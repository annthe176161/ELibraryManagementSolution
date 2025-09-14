using ELibraryManagement.Api.Models;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IUserStatusService
    {
        Task<UserStatus> GetUserStatusAsync(string userId);
        Task<UserStatus> CreateUserStatusAsync(string userId);
        Task UpdateUserStatusAsync(UserStatus userStatus);
        Task IncrementBorrowCountAsync(string userId);
        Task DecrementBorrowCountAsync(string userId);
        Task AddFineAsync(string userId, decimal fineAmount);
        Task PayFineAsync(string userId, decimal paymentAmount);
        Task BlockUserAsync(string userId, string reason, DateTime? blockedUntil = null);
        Task UnblockUserAsync(string userId);
        Task<bool> CanUserBorrowAsync(string userId);
    }
}