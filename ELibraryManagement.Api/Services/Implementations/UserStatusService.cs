using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using ELibraryManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class UserStatusService : IUserStatusService
    {
        private readonly ApplicationDbContext _context;

        public UserStatusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserStatus> GetUserStatusAsync(string userId)
        {
            var userStatus = await _context.UserStatuses.FindAsync(userId);

            if (userStatus == null)
            {
                // Create default user status if not exists
                userStatus = await CreateUserStatusAsync(userId);
            }

            return userStatus;
        }

        public async Task<UserStatus> CreateUserStatusAsync(string userId)
        {
            var userStatus = new UserStatus
            {
                UserId = userId,
                AccountStatus = UserAccountStatus.Active,
                TotalOutstandingFines = 0,
                OverdueFinesCount = 0,
                MaxBorrowLimit = 5,
                CurrentBorrowCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserStatuses.Add(userStatus);
            await _context.SaveChangesAsync();

            return userStatus;
        }

        public async Task UpdateUserStatusAsync(UserStatus userStatus)
        {
            userStatus.UpdatedAt = DateTime.UtcNow;
            _context.UserStatuses.Update(userStatus);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementBorrowCountAsync(string userId)
        {
            var userStatus = await GetUserStatusAsync(userId);
            userStatus.CurrentBorrowCount++;
            await UpdateUserStatusAsync(userStatus);
        }

        public async Task DecrementBorrowCountAsync(string userId)
        {
            var userStatus = await GetUserStatusAsync(userId);
            if (userStatus.CurrentBorrowCount > 0)
            {
                userStatus.CurrentBorrowCount--;
            }
            await UpdateUserStatusAsync(userStatus);
        }

        public async Task AddFineAsync(string userId, decimal fineAmount)
        {
            var userStatus = await GetUserStatusAsync(userId);
            userStatus.TotalOutstandingFines += fineAmount;
            userStatus.OverdueFinesCount++;

            // Block user if too many fines
            if (userStatus.TotalOutstandingFines > 100000) // 100k VND
            {
                userStatus.AccountStatus = UserAccountStatus.Blocked;
                userStatus.BlockReason = $"Outstanding fines exceed limit: {userStatus.TotalOutstandingFines:N0} VND";
            }

            await UpdateUserStatusAsync(userStatus);
        }

        public async Task PayFineAsync(string userId, decimal paymentAmount)
        {
            var userStatus = await GetUserStatusAsync(userId);
            userStatus.TotalOutstandingFines -= paymentAmount;

            if (userStatus.TotalOutstandingFines < 0)
            {
                userStatus.TotalOutstandingFines = 0;
            }

            // Unblock if fines are paid
            if (userStatus.TotalOutstandingFines == 0 && userStatus.AccountStatus == UserAccountStatus.Blocked)
            {
                userStatus.AccountStatus = UserAccountStatus.Active;
                userStatus.BlockReason = null;
                userStatus.BlockedUntil = null;
            }

            await UpdateUserStatusAsync(userStatus);
        }

        public async Task BlockUserAsync(string userId, string reason, DateTime? blockedUntil = null)
        {
            var userStatus = await GetUserStatusAsync(userId);
            userStatus.AccountStatus = UserAccountStatus.Blocked;
            userStatus.BlockReason = reason;
            userStatus.BlockedUntil = blockedUntil;
            await UpdateUserStatusAsync(userStatus);
        }

        public async Task UnblockUserAsync(string userId)
        {
            var userStatus = await GetUserStatusAsync(userId);
            userStatus.AccountStatus = UserAccountStatus.Active;
            userStatus.BlockReason = null;
            userStatus.BlockedUntil = null;
            await UpdateUserStatusAsync(userStatus);
        }

        public async Task<bool> CanUserBorrowAsync(string userId)
        {
            var userStatus = await GetUserStatusAsync(userId);

            // Check if user is blocked
            if (userStatus.AccountStatus == UserAccountStatus.Blocked)
            {
                return false;
            }

            // Check if user has reached borrow limit
            if (userStatus.CurrentBorrowCount >= userStatus.MaxBorrowLimit)
            {
                return false;
            }

            // Check if user has too many outstanding fines
            if (userStatus.TotalOutstandingFines > 50000) // 50k VND limit
            {
                return false;
            }

            return true;
        }
    }
}