using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services;
using ELibraryManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class OverdueProcessingService : IOverdueProcessingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OverdueProcessingService> _logger;
        private readonly OverdueSettings _overdueSettings;

        public OverdueProcessingService(
            ApplicationDbContext context,
            ILogger<OverdueProcessingService> logger,
            IOptions<OverdueSettings> overdueSettings)
        {
            _context = context;
            _logger = logger;
            _overdueSettings = overdueSettings.Value;
        }

        public async Task<int> ProcessOverdueBooksAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Bắt đầu tìm kiếm sách quá hạn...");

                // Tìm tất cả borrow records đang mượn hoặc đã quá hạn nhưng chưa trả
                // QUAN TRỌNG: Không bỏ qua những records đã có phạt để có thể cập nhật phạt theo số ngày quá hạn thực tế
                var overdueBorrowRecords = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .Include(br => br.Fines)
                    .Where(br => (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue)
                              && br.ReturnDate == null
                              && br.DueDate < DateTime.UtcNow)
                    .ToListAsync();

                _logger.LogInformation($"📋 Tìm thấy {overdueBorrowRecords.Count} borrow records quá hạn");

                int processedCount = 0;

                foreach (var borrowRecord in overdueBorrowRecords)
                {
                    // Use Vietnam local date to calculate days-overdue (date-only) to avoid off-by-one due to timezones
                    var overdueDays = (DateTimeHelper.VietnamNow().Date - borrowRecord.DueDate.ToVietnamTime().Date).Days;
                    _logger.LogInformation($"📖 Xử lý borrow record ID {borrowRecord.Id}: {borrowRecord.Book.Title} - quá hạn {overdueDays} ngày");

                    var success = await ProcessSingleBorrowRecordAsync(borrowRecord.Id);
                    if (success)
                    {
                        processedCount++;
                    }
                }

                _logger.LogInformation($"✅ Hoàn thành: Đã xử lý {processedCount}/{overdueBorrowRecords.Count} borrow records quá hạn");
                return processedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi xử lý sách quá hạn tự động");
                return 0;
            }
        }

        public async Task<bool> ProcessSingleBorrowRecordAsync(int borrowRecordId)
        {
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .Include(br => br.Fines)
                    .FirstOrDefaultAsync(br => br.Id == borrowRecordId);

                if (borrowRecord == null)
                {
                    _logger.LogWarning($"Không tìm thấy borrow record với ID: {borrowRecordId}");
                    return false;
                }

                // Kiểm tra điều kiện quá hạn
                if ((borrowRecord.Status != BorrowStatus.Borrowed && borrowRecord.Status != BorrowStatus.Overdue) ||
                    borrowRecord.ReturnDate != null ||
                    borrowRecord.DueDate >= DateTime.UtcNow)
                {
                    return false; // Không cần xử lý
                }

                var overdueDays = (DateTimeHelper.VietnamNow().Date - borrowRecord.DueDate.ToVietnamTime().Date).Days;

                // Cập nhật trạng thái thành Overdue (nếu chưa phải)
                if (borrowRecord.Status != BorrowStatus.Overdue)
                {
                    borrowRecord.Status = BorrowStatus.Overdue;
                    borrowRecord.UpdatedAt = DateTime.UtcNow;
                }

                // Kiểm tra xem đã có fine chưa thanh toán cho borrow record này chưa
                var existingFine = borrowRecord.Fines
                    .FirstOrDefault(f => f.Reason.Contains("Quá hạn") && f.Status == FineStatus.Pending);

                var fineAmount = CalculateFineAmount(overdueDays);
                var isNewFine = false;
                var oldAmount = 0m;

                if (existingFine == null)
                {
                    // Tạo fine mới
                    var fine = new Fine
                    {
                        UserId = borrowRecord.UserId, // Thêm UserId từ BorrowRecord
                        BorrowRecordId = borrowRecord.Id,
                        Amount = fineAmount,
                        Reason = $"Quá hạn {overdueDays} ngày - Sách: {borrowRecord.Book.Title}",
                        Status = FineStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                        // Không cần DueDate - phạt tính liên tục theo số ngày quá hạn
                    };

                    _context.Fines.Add(fine);
                    isNewFine = true;
                    existingFine = fine; // Để sử dụng cho việc ghi lịch sử sau

                    _logger.LogInformation($"✨ Tạo phạt mới cho borrow record {borrowRecordId}: {fineAmount:N0} VND ({overdueDays} ngày)");
                }
                else
                {
                    // Cập nhật fine hiện tại theo số ngày quá hạn thực tế
                    oldAmount = existingFine.Amount;
                    if (existingFine.Amount != fineAmount)
                    {
                        existingFine.Amount = fineAmount;
                        existingFine.Reason = $"Quá hạn {overdueDays} ngày - Sách: {borrowRecord.Book.Title}";
                        existingFine.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"🔄 Cập nhật phạt cho borrow record {borrowRecordId}: {oldAmount:N0} VND → {fineAmount:N0} VND ({overdueDays} ngày)");
                    }
                    else
                    {
                        _logger.LogInformation($"✅ Phạt cho borrow record {borrowRecordId} đã đúng: {fineAmount:N0} VND ({overdueDays} ngày) - không cần cập nhật");
                        return true; // Không cần lưu thay đổi
                    }
                }

                await _context.SaveChangesAsync();

                // Ghi log lịch sử sau khi đã có FineId
                if (isNewFine)
                {
                    var fineHistory = new FineActionHistory
                    {
                        FineId = existingFine.Id,
                        UserId = borrowRecord.UserId,
                        ActionType = FineActionType.ReminderSent,
                        Description = $"Tạo phạt quá hạn {overdueDays} ngày",
                        Amount = fineAmount,
                        Notes = $"Phạt được tạo tự động do sách quá hạn",
                        ActionDate = DateTime.UtcNow
                    };
                    _context.FineActionHistories.Add(fineHistory);
                    // Update reminder counters on the fine so UI/statistics reflect reminder actions
                    existingFine.ReminderCount += 1;
                    existingFine.LastReminderDate = DateTime.UtcNow;
                }
                else if (oldAmount != fineAmount)
                {
                    var fineHistory = new FineActionHistory
                    {
                        FineId = existingFine.Id,
                        UserId = borrowRecord.UserId,
                        ActionType = FineActionType.ReminderSent,
                        Description = $"Cập nhật phạt từ {oldAmount:N0} VND lên {fineAmount:N0} VND do tăng thêm ngày quá hạn",
                        Amount = fineAmount,
                        Notes = $"Cập nhật tự động - hiện tại quá hạn {overdueDays} ngày",
                        ActionDate = DateTime.UtcNow
                    };
                    _context.FineActionHistories.Add(fineHistory);
                    // Update reminder metadata so reminder count and date are accurate
                    existingFine.ReminderCount += 1;
                    existingFine.LastReminderDate = DateTime.UtcNow;
                }

                // Lưu lịch sử nếu có
                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Đã xử lý borrow record {borrowRecordId} quá hạn {overdueDays} ngày");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xử lý borrow record {borrowRecordId}");
                return false;
            }
        }

        public decimal CalculateFineAmount(int overdueDays)
        {
            if (overdueDays <= 0)
                return 0;

            // Tính phạt cố định: 5,000 VNĐ/ngày
            // Phạt tiếp tục tính theo số ngày quá hạn thực tế, không bị giới hạn tối đa
            decimal totalFine = overdueDays * _overdueSettings.DailyFine;

            return totalFine;
        }
    }

    public class OverdueSettings
    {
        public decimal DailyFine { get; set; } = 5000; // 5,000 VNĐ/ngày - phạt tính liên tục theo số ngày quá hạn thực tế
    }
}