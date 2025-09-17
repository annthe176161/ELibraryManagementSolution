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

        public async Task<bool> SendBookDueReminderAsync(string email, string userName, string bookTitle, DateTime dueDate, int daysLeft, bool canExtend)
        {
            var subject = $"Nhắc nhở trả sách - {bookTitle}";

            var extensionSection = canExtend
                ? $@"
                    <div style='background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                        <h4 style='color: #155724; margin-top: 0;'>💡 Bạn có thể gia hạn sách!</h4>
                        <p style='color: #155724; margin: 10px 0;'>Nếu cần thêm thời gian, bạn có thể gia hạn sách này (tối đa 2 lần).</p>
                        <div style='text-align: center; margin: 15px 0;'>
                            <a href='https://localhost:7208/Borrow/MyBorrows' 
                               style='background-color: #28a745; color: white; padding: 10px 25px; text-decoration: none; border-radius: 20px; font-weight: bold; display: inline-block;'>
                                🔄 Gia hạn sách ngay
                            </a>
                        </div>
                    </div>
                "
                : $@"
                    <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                        <h4 style='color: #721c24; margin-top: 0;'>⚠️ Không thể gia hạn</h4>
                        <p style='color: #721c24; margin: 10px 0;'>Sách này đã đạt giới hạn gia hạn (2 lần) hoặc đã quá hạn. Vui lòng trả sách đúng hạn.</p>
                    </div>
                ";

            var urgencyColor = daysLeft <= 1 ? "#dc3545" : "#ffc107";
            var urgencyText = daysLeft <= 1 ? "KHẨN CẤP" : "THÔNG BÁO";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8f9fa; padding: 20px;'>
                    <div style='background-color: {urgencyColor}; color: white; text-align: center; padding: 15px; border-radius: 10px 10px 0 0;'>
                        <h2 style='margin: 0; font-size: 24px;'>📚 {urgencyText} TRẢ SÁCH</h2>
                    </div>
                    
                    <div style='background-color: white; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #ddd;'>
                        <h3 style='color: #333; margin-top: 0;'>Xin chào {userName}!</h3>
                        
                        <div style='background-color: #f1f3f4; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h4 style='color: #333; margin-top: 0;'>📖 Thông tin sách:</h4>
                            <p style='margin: 5px 0;'><strong>Tên sách:</strong> {bookTitle}</p>
                            <p style='margin: 5px 0;'><strong>Hạn trả:</strong> <span style='color: {urgencyColor}; font-weight: bold;'>{dueDate:dd/MM/yyyy}</span></p>
                            <p style='margin: 5px 0;'><strong>Thời gian còn lại:</strong> <span style='color: {urgencyColor}; font-weight: bold;'>{daysLeft} ngày</span></p>
                        </div>

                        {extensionSection}

                        <div style='background-color: #e3f2fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h4 style='color: #1976d2; margin-top: 0;'>🔔 Lưu ý quan trọng:</h4>
                            <ul style='margin: 10px 0; padding-left: 20px;'>
                                <li>Vui lòng trả sách đúng hạn để tránh phí phạt</li>
                                <li>Phí phạt: 5,000 VND/ngày cho mỗi ngày trễ hạn</li>
                                <li>Sách quá hạn sẽ ảnh hướng đến khả năng mượn sách trong tương lai</li>
                                <li>Mỗi sinh viên chỉ được gia hạn tối đa 2 lần cho mỗi cuốn sách</li>
                            </ul>
                        </div>

                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://localhost:7208/Borrow/MyBorrows' 
                               style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; margin: 5px;'>
                                🔍 Xem danh sách sách đang mượn
                            </a>
                        </div>

                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                        
                        <p style='color: #666; font-size: 14px; text-align: center; margin: 20px 0;'>
                            Email này được gửi tự động từ hệ thống Thư viện ELibrary.<br>
                            Nếu có thắc mắc, vui lòng liên hệ thủ thư hoặc trả lời email này.
                        </p>
                        
                        <div style='text-align: center; margin-top: 30px;'>
                            <p style='margin: 0; font-weight: bold; color: #333;'>Trân trọng,</p>
                            <p style='margin: 5px 0; color: #007bff; font-weight: bold;'>Đội ngũ Thư viện ELibrary</p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }
    }
}