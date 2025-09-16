using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class BorrowStatusValidationService : IBorrowStatusValidationService
    {
        private readonly Dictionary<BorrowStatus, HashSet<BorrowStatus>> _allowedTransitions;

        public BorrowStatusValidationService()
        {
            // Định nghĩa các chuyển đổi trạng thái hợp lệ
            _allowedTransitions = new Dictionary<BorrowStatus, HashSet<BorrowStatus>>
            {
                // Từ Requested có thể chuyển sang Borrowed (phê duyệt) hoặc Cancelled (từ chối)
                [BorrowStatus.Requested] = new HashSet<BorrowStatus>
                {
                    BorrowStatus.Borrowed,
                    BorrowStatus.Cancelled
                },

                // Từ Borrowed có thể chuyển sang Returned, Lost, Damaged, hoặc Cancelled
                [BorrowStatus.Borrowed] = new HashSet<BorrowStatus>
                {
                    BorrowStatus.Returned,
                    BorrowStatus.Lost,
                    BorrowStatus.Damaged,
                    BorrowStatus.Cancelled
                },

                // Các trạng thái cuối không thể chuyển tiếp
                [BorrowStatus.Returned] = new HashSet<BorrowStatus>(),
                [BorrowStatus.Lost] = new HashSet<BorrowStatus>(),
                [BorrowStatus.Damaged] = new HashSet<BorrowStatus>(),
                [BorrowStatus.Cancelled] = new HashSet<BorrowStatus>()
            };
        }

        public bool CanTransition(BorrowStatus currentStatus, BorrowStatus newStatus)
        {
            // Không thể chuyển sang cùng trạng thái
            if (currentStatus == newStatus)
                return false;

            // Kiểm tra xem chuyển đổi có được phép không
            return _allowedTransitions.ContainsKey(currentStatus) &&
                   _allowedTransitions[currentStatus].Contains(newStatus);
        }

        public IEnumerable<BorrowStatus> GetAllowedTransitions(BorrowStatus currentStatus)
        {
            return _allowedTransitions.ContainsKey(currentStatus)
                ? _allowedTransitions[currentStatus]
                : Enumerable.Empty<BorrowStatus>();
        }

        public bool IsFinalStatus(BorrowStatus status)
        {
            return _allowedTransitions.ContainsKey(status) &&
                   _allowedTransitions[status].Count == 0;
        }

        public string GetTransitionErrorMessage(BorrowStatus currentStatus, BorrowStatus newStatus)
        {
            if (currentStatus == newStatus)
            {
                return $"Không thể chuyển sang cùng trạng thái '{GetStatusDisplayName(currentStatus)}'";
            }

            if (IsFinalStatus(currentStatus))
            {
                return $"Không thể chuyển từ trạng thái cuối '{GetStatusDisplayName(currentStatus)}' sang trạng thái khác";
            }

            if (!CanTransition(currentStatus, newStatus))
            {
                var allowedTransitions = GetAllowedTransitions(currentStatus);
                if (allowedTransitions.Any())
                {
                    var allowedNames = allowedTransitions.Select(s => GetStatusDisplayName(s));
                    return $"Không thể chuyển từ '{GetStatusDisplayName(currentStatus)}' sang '{GetStatusDisplayName(newStatus)}'. " +
                           $"Các trạng thái hợp lệ: {string.Join(", ", allowedNames)}";
                }
                else
                {
                    return $"Trạng thái '{GetStatusDisplayName(currentStatus)}' là trạng thái cuối và không thể chuyển tiếp";
                }
            }

            return string.Empty;
        }

        private string GetStatusDisplayName(BorrowStatus status)
        {
            return status switch
            {
                BorrowStatus.Requested => "Chờ duyệt",
                BorrowStatus.Borrowed => "Đang mượn",
                BorrowStatus.Returned => "Đã trả",
                BorrowStatus.Lost => "Mất sách",
                BorrowStatus.Damaged => "Hư hỏng",
                BorrowStatus.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}