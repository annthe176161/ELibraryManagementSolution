namespace ELibraryManagement.Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
    }
}