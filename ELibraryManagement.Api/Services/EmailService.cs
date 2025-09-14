using System.Net;
using System.Net.Mail;

namespace ELibraryManagement.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"] ?? "587"),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        emailSettings["FromEmail"] ?? emailSettings["Username"],
                        emailSettings["FromName"]
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Xác nhận tài khoản - ELibrary Management";
            var body = $@"
                <h2>Chào mừng bạn đến với ELibrary Management System!</h2>
                <p>Vui lòng xác nhận địa chỉ email của bạn bằng cách nhấp vào liên kết bên dưới:</p>
                <p><a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Xác nhận Email</a></p>
                <p>Nếu bạn không thể nhấp vào liên kết, vui lòng sao chép và dán URL sau vào trình duyệt:</p>
                <p>{confirmationLink}</p>
                <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ ELibrary Management</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Đặt lại mật khẩu - ELibrary Management";
            var body = $@"
                <h2>Yêu cầu đặt lại mật khẩu</h2>
                <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại ELibrary Management System.</p>
                <p>Vui lòng nhấp vào liên kết bên dưới để đặt lại mật khẩu:</p>
                <p><a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a></p>
                <p>Nếu bạn không thể nhấp vào liên kết, vui lòng sao chép và dán URL sau vào trình duyệt:</p>
                <p>{resetLink}</p>
                <p><strong>Lưu ý quan trọng:</strong></p>
                <ul>
                    <li>Liên kết này sẽ hết hạn sau 1 giờ</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                    <li>Không chia sẻ liên kết này với bất kỳ ai khác</li>
                </ul>
                <br>
                <p>Trân trọng,</p>
                <p>Đội ngũ ELibrary Management</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }
    }
}