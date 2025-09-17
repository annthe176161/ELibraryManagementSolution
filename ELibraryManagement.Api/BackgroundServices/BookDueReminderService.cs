using Microsoft.EntityFrameworkCore;
using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Models;

namespace ELibraryManagement.Api.BackgroundServices
{
    public class BookDueReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookDueReminderService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Chạy mỗi 24 giờ

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

            // Lấy danh sách sách sắp hết hạn (3 ngày, 1 ngày và hôm nay)
            var today = DateTime.UtcNow.Date;
            var threeDaysFromNow = today.AddDays(3);
            var oneDayFromNow = today.AddDays(1);

            var borrowsNearingDue = await context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => br.Status == BorrowStatus.Borrowed &&
                           br.ReturnDate == null &&
                           (br.DueDate.Date == threeDaysFromNow ||
                            br.DueDate.Date == oneDayFromNow ||
                            br.DueDate.Date == today))
                .ToListAsync();

            foreach (var borrow in borrowsNearingDue)
            {
                try
                {
                    // Kiểm tra xem đã gửi email nhắc nhở trong ngày chưa
                    var reminderKey = $"REMINDER_{borrow.Id}_{DateTime.UtcNow:yyyy-MM-dd}";
                    if (borrow.Notes?.Contains(reminderKey) == true)
                    {
                        continue; // Đã gửi email trong ngày này rồi
                    }

                    var daysLeft = (borrow.DueDate.Date - today).Days;
                    var userName = $"{borrow.User.FirstName} {borrow.User.LastName}".Trim();

                    if (string.IsNullOrEmpty(userName))
                    {
                        userName = borrow.User.Email.Split('@')[0];
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
                        // Cập nhật notes để đánh dấu đã gửi email
                        borrow.Notes = $"{borrow.Notes}\n{reminderKey} - Email nhắc nhở đã gửi";
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