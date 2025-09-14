using ELibraryManagement.Api.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> GetCurrentUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
        Task<bool> AssignRoleAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);
        Task<AuthResponseDto> ForgotPasswordAsync(string email);
        Task<AuthResponseDto> ResetPasswordAsync(string email, string token, string newPassword);
        Task<AuthResponseDto> HandleGoogleLoginAsync(ExternalLoginInfo info);
        Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponseDto> ResendEmailConfirmationAsync(string email);
    }
}
