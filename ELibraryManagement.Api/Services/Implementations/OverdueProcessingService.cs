using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services;
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

                // Tìm tất cả borrow records đang mượn hoặc đã quá hạn nhưng chưa trả và chưa có phạt
                var overdueBorrowRecords = await _context.BorrowRecords
                    .Include(br => br.User)
                    .Include(br => br.Book)
                    .Include(br => br.Fines)
                    .Where(br => (br.Status == BorrowStatus.Borrowed || br.Status == BorrowStatus.Overdue)
                              && br.ReturnDate == null
                              && br.DueDate < DateTime.UtcNow
                              && !br.Fines.Any()) // Chỉ xử lý records chưa có phạt
                    .ToListAsync();

                _logger.LogInformation($"📋 Tìm thấy {overdueBorrowRecords.Count} borrow records quá hạn");

                int processedCount = 0;

                foreach (var borrowRecord in overdueBorrowRecords)
                {
                    var overdueDays = (DateTime.UtcNow - borrowRecord.DueDate).Days;
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

                var overdueDays = (DateTime.UtcNow - borrowRecord.DueDate).Days;

                // Cập nhật trạng thái thành Overdue (nếu chưa phải)
                if (borrowRecord.Status != BorrowStatus.Overdue)
                {
                    borrowRecord.Status = BorrowStatus.Overdue;
                    borrowRecord.UpdatedAt = DateTime.UtcNow;
                }

                // Kiểm tra xem đã có fine cho borrow record này chưa
                var existingFine = borrowRecord.Fines
                    .FirstOrDefault(f => f.Reason.Contains("Quá hạn") && f.Status == FineStatus.Pending);

                if (existingFine == null)
                {
                    // Tạo fine mới
                    var fineAmount = CalculateFineAmount(overdueDays);
                    var fine = new Fine
                    {
                        UserId = borrowRecord.UserId, // Thêm UserId từ BorrowRecord
                        BorrowRecordId = borrowRecord.Id,
                        Amount = fineAmount,
                        Reason = $"Quá hạn {overdueDays} ngày - Sách: {borrowRecord.Book.Title}",
                        Status = FineStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(_overdueSettings.FinePaymentDueDays)
                    };

                    _context.Fines.Add(fine);

                    _logger.LogInformation($"Tạo phạt mới cho borrow record {borrowRecordId}: {fineAmount:C}");
                }
                else
                {
                    // Cập nhật fine hiện tại nếu số tiền thay đổi
                    var newFineAmount = CalculateFineAmount(overdueDays);
                    if (existingFine.Amount != newFineAmount)
                    {
                        existingFine.Amount = newFineAmount;
                        existingFine.Reason = $"Quá hạn {overdueDays} ngày - Sách: {borrowRecord.Book.Title}";
                        existingFine.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Cập nhật phạt cho borrow record {borrowRecordId}: {newFineAmount:C}");
                    }
                }

                await _context.SaveChangesAsync();

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
            decimal totalFine = overdueDays * _overdueSettings.DailyFine;

            // Giới hạn phạt tối đa
            return Math.Min(totalFine, _overdueSettings.MaxFineAmount);
        }
    }

    public class OverdueSettings
    {
        public decimal DailyFine { get; set; } = 5000; // 5,000 VNĐ/ngày

        public decimal MaxFineAmount { get; set; } = 500000; // Phạt tối đa 500,000 VNĐ

        public int FinePaymentDueDays { get; set; } = 30; // Hạn thanh toán phạt: 30 ngày
    }
}