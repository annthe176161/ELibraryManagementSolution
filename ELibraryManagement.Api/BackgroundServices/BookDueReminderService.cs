using Microsoft.EntityFrameworkCore;
using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Helpers;

namespace ELibraryManagement.Api.BackgroundServices
{
    public class BookDueReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookDueReminderService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // Chạy mỗi 1 phút để test

        public BookDueReminderService(IServiceProvider serviceProvider, ILogger<BookDueReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendDueReminders();
                    _logger.LogInformation("Book due reminder service executed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing book due reminder service");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task SendDueReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Lấy thời gian hiện tại theo UTC và chuyển sang múi giờ Việt Nam để tính "ngày Việt Nam"
            var nowUtc = DateTime.UtcNow;

            // Chọn timezone id phù hợp: trên Windows là "SE Asia Standard Time", trên Linux/macOS là "Asia/Ho_Chi_Minh".
            TimeZoneInfo? vietnamTz = null;
            try
            {
                vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                try
                {
                    vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                }
                catch
                {
                    // Fallback: use UTC (safer than throwing) — but log a warning
                    vietnamTz = TimeZoneInfo.Utc;
                    _logger.LogWarning("Could not find Vietnam timezone on this system; falling back to UTC for reminder date calculations.");
                }
            }

            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vietnamTz!);
            var vietnamToday = vietnamNow.Date;
            var vietnamOneDay = vietnamToday.AddDays(1);
            var vietnamThreeDays = vietnamToday.AddDays(3);

            // Convert Vietnam local-day boundaries back to UTC so EF query can compare UTC DueDate columns in SQL.
            DateTime startTodayUtc = TimeZoneInfo.ConvertTimeToUtc(vietnamToday, vietnamTz!);
            DateTime endTodayUtc = startTodayUtc.AddDays(1);

            DateTime startOneDayUtc = TimeZoneInfo.ConvertTimeToUtc(vietnamOneDay, vietnamTz!);
            DateTime endOneDayUtc = startOneDayUtc.AddDays(1);

            DateTime startThreeDayUtc = TimeZoneInfo.ConvertTimeToUtc(vietnamThreeDays, vietnamTz!);
            DateTime endThreeDayUtc = startThreeDayUtc.AddDays(1);

            // Chỉ lấy những bản ghi có trạng thái Borrowed, chưa trả, DueDate vẫn còn lớn hơn giờ UTC hiện tại (không quá hạn),
            // và có DueDate thuộc một trong các ngày mục tiêu theo ngày Việt Nam (3 ngày, 1 ngày, hôm nay)
            var borrowsNearingDue = await context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => br.Status == BorrowStatus.Borrowed
                             && br.ReturnDate == null
                             && br.DueDate > nowUtc
                             && ((br.DueDate >= startThreeDayUtc && br.DueDate < endThreeDayUtc)
                                 || (br.DueDate >= startOneDayUtc && br.DueDate < endOneDayUtc)
                                 || (br.DueDate >= startTodayUtc && br.DueDate < endTodayUtc)))
                .ToListAsync();

            foreach (var borrow in borrowsNearingDue)
            {
                try
                {
                    // Kiểm tra thời điểm gửi email nhắc nhở trước đó (nếu có) trong borrow.Notes
                    // Format lưu là: REMINDER_{borrowId}_{utc-iso}
                    var nowUtcCheck = DateTime.UtcNow;
                    var reminderPrefix = $"REMINDER_{borrow.Id}_";
                    DateTime? lastSentUtc = null;
                    if (!string.IsNullOrEmpty(borrow.Notes))
                    {
                        // Tìm phần tử REMINDER_{id}_... gần nhất (lấy lần xuất hiện cuối cùng)
                        var idx = borrow.Notes.LastIndexOf(reminderPrefix, StringComparison.Ordinal);
                        if (idx >= 0)
                        {
                            var start = idx + reminderPrefix.Length;
                            // lấy đến cuối dòng
                            var end = borrow.Notes.IndexOf('\n', start);
                            var token = end >= 0 ? borrow.Notes.Substring(start, end - start) : borrow.Notes.Substring(start);
                            // token có dạng ISO time hoặc kèm mô tả, lấy phần đầu (trước dấu cách) nếu cần
                            var firstPart = token.Split(' ')[0].Trim();
                            if (DateTime.TryParse(firstPart, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed))
                            {
                                lastSentUtc = parsed.ToUniversalTime();
                            }
                        }
                    }

                    // Nếu đã gửi trước đó và chưa đủ thời gian chờ (_period) thì bỏ qua
                    if (lastSentUtc.HasValue && nowUtcCheck - lastSentUtc.Value < _period)
                    {
                        continue; // chưa tới lượt gửi lại
                    }

                    // Tính daysLeft theo ngày Việt Nam: convert due date (UTC) -> Vietnam local date
                    var dueInVietnam = TimeZoneInfo.ConvertTimeFromUtc(borrow.DueDate, vietnamTz).Date;
                    var daysLeft = (dueInVietnam - vietnamToday).Days;
                    // Ensure required related data exists
                    if (borrow.User == null || string.IsNullOrWhiteSpace(borrow.User.Email) || borrow.Book == null || string.IsNullOrWhiteSpace(borrow.Book.Title))
                    {
                        _logger.LogWarning("Skipping reminder for borrow {borrowId} because user or book info is missing", borrow.Id);
                        continue;
                    }

                    var userName = $"{borrow.User.FirstName} {borrow.User.LastName}".Trim();
                    if (string.IsNullOrEmpty(userName))
                    {
                        // fallback to email local-part
                        var at = borrow.User.Email.IndexOf('@');
                        userName = at > 0 ? borrow.User.Email.Substring(0, at) : borrow.User.Email;
                    }

                    var emailSent = await emailService.SendBookDueReminderAsync(
                        borrow.User.Email,
                        userName,
                        borrow.Book.Title,
                        borrow.DueDate,
                        daysLeft,
                        borrow.CanExtend
                    );

                    if (emailSent)
                    {
                        // Cập nhật notes để đánh dấu thời điểm đã gửi (lưu UTC ISO)
                        var sentMarker = $"REMINDER_{borrow.Id}_{DateTime.UtcNow:o} - Email nhắc nhở đã gửi";
                        borrow.Notes = string.IsNullOrEmpty(borrow.Notes) ? sentMarker : borrow.Notes + "\n" + sentMarker;
                        borrow.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();

                        _logger.LogInformation("Sent due reminder email to {email} for book: {bookTitle}",
                            borrow.User.Email, borrow.Book.Title);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send due reminder email to {email} for book: {bookTitle}",
                            borrow.User.Email, borrow.Book.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder email for borrow record {borrowId}", borrow.Id);
                }
            }

            _logger.LogInformation("Processed {count} due reminders", borrowsNearingDue.Count);
        }
    }
}