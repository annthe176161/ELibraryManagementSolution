using ELibraryManagement.Web.Models;

namespace ELibraryManagement.Web.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model);
        Task<AuthResponseViewModel> LoginAsync(LoginViewModel model);
        Task<UserViewModel?> GetCurrentUserAsync();
        void Logout();
        bool IsAuthenticated();
        string? GetCurrentUserToken();
        string GetCurrentUserName();
        string? GetCurrentToken();
        Task<List<string>> GetCurrentUserRolesAsync();
        Task<bool> IsInRoleAsync(string roleName);
        Task<AuthResponseViewModel> UpdateProfileAsync(EditProfileViewModel model);
        Task<AuthResponseViewModel> ChangePasswordAsync(ChangePasswordViewModel model);
        Task<AuthResponseViewModel> ForgotPasswordAsync(ForgotPasswordViewModel model);
        Task<AuthResponseViewModel> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResponseViewModel> UploadAvatarAsync(IFormFile file);
        Task<AuthResponseViewModel> ResendEmailConfirmationAsync(ResendEmailConfirmationViewModel model);
        void StoreToken(string token);
        void StoreUserSession(string token, UserViewModel user);
    }
}