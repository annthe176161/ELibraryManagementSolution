# Email Configuration Guide

## Cấu hình Email cho tính năng Quên mật khẩu

### 1. Cài đặt trong appsettings.json (ELibraryManagement.Api)

Bạn cần cập nhật phần `EmailSettings` trong file `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "FromEmail": "your-email@gmail.com",
  "FromName": "ELibrary Management System",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "EnableSsl": true
}
```

### 2. Thiết lập Gmail App Password

Để sử dụng Gmail SMTP:

1. Truy cập [Google Account Settings](https://myaccount.google.com/)
2. Chọn "Security" → "2-Step Verification" (bật nếu chưa có)
3. Chọn "App passwords"
4. Tạo app password mới cho ứng dụng
5. Sử dụng app password này thay vì mật khẩu Gmail thường

#### Ghi chú: Sử dụng App Password và biến môi trường (PowerShell)

Nếu bạn dùng Gmail với App Password, tránh lưu mật khẩu vào git. Thay vào đó, khi chạy API từ PowerShell bạn có thể set biến môi trường tạm thời như sau:

```powershell
$env:EmailSettings__FromEmail = "your-email@gmail.com";
$env:EmailSettings__Username = "your-email@gmail.com";
$env:EmailSettings__Password = "your-app-password";
dotnet run --project .\ELibraryManagement.Api\ELibraryManagement.Api.csproj
```

Sau khi đóng PowerShell, các biến này sẽ biến mất (tạm thời), nên an toàn hơn là hardcoding trong `appsettings.Development.json`.

### 3. Các nhà cung cấp email khác

#### Outlook/Hotmail:

```json
"EmailSettings": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "FromEmail": "your-email@outlook.com",
  "Username": "your-email@outlook.com",
  "Password": "your-password",
  "EnableSsl": true
}
```

#### Yahoo Mail:

```json
"EmailSettings": {
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "FromEmail": "your-email@yahoo.com",
  "Username": "your-email@yahoo.com",
  "Password": "your-app-password",
  "EnableSsl": true
}
```

### 4. Test Email Functionality

1. Đảm bảo email settings đã được cấu hình đúng
2. Khởi động API project
3. Truy cập trang Forgot Password: `https://localhost:7208/Account/ForgotPassword`
4. Nhập email đã đăng ký trong hệ thống
5. Kiểm tra hộp thư (bao gồm spam folder)

### 4b. Test nhanh bằng email dev tools (không cần tài khoản thật)

Nếu bạn không muốn dùng tài khoản email thật trong môi trường phát triển, có thể dùng một trong các công cụ sau để bắt và xem email cục bộ:

- Papercut (Windows) — giao diện đơn giản, chặn mọi email gửi đến và hiển thị HTML/plain text. Tải về và chạy Papercut, sau đó cấu hình SMTP server trong `appsettings.Development.json` như sau:

```json
"EmailSettings": {
  "SmtpServer": "localhost",
  "SmtpPort": 25,
  "FromEmail": "no-reply@example.com",
  "FromName": "ELibrary Management System",
  "Username": "",
  "Password": "",
  "EnableSsl": false
}
```

- smtp4dev — Docker hoặc Windows binary, giao diện web để xem mail. Mặc định lắng nghe 25 hoặc 2525.

- Mailtrap (cloud) — tạo tài khoản miễn phí, nhận SMTP credentials (host, port, username, password). Dùng Mailtrap khi muốn test gửi email thực tế nhưng không gửi ra ngoài.

### 4c. Test bằng Mailtrap (ví dụ)

1. Tạo tài khoản Mailtrap (https://mailtrap.io/)
2. Tạo inbox mới và copy SMTP settings (host, port, user, pass)
3. Cập nhật `appsettings.Development.json` `EmailSettings` với những giá trị đó (FromEmail vẫn nên là địa chỉ hợp lệ như `no-reply@yourdomain.test`).
4. Khởi động API và thực hiện đăng ký tài khoản test. Mở Mailtrap inbox để xem email được nhận.

### 5. Troubleshooting

**Lỗi thường gặp:**

1. **"Authentication failed"**

   - Kiểm tra username/password
   - Đảm bảo sử dụng App Password cho Gmail
   - Kiểm tra 2FA đã được bật

2. **"SMTP server not found"**

   - Kiểm tra SmtpServer và SmtpPort
   - Đảm bảo kết nối internet ổn định

3. **"Email not sent"**
   - Kiểm tra logs trong console/file log
   - Xác nhận FromEmail hợp lệ
   - Kiểm tra EnableSsl setting

### 6. Bảo mật

- Không commit thông tin email credentials vào Git
- Sử dụng Environment Variables trong production
- Cân nhắc sử dụng email service như SendGrid, AWS SES cho production

### 7. Environment Variables (Production)

```bash
EmailSettings__FromEmail=your-email@gmail.com
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-app-password
```

Thêm vào `appsettings.Production.json`:

```json
"EmailSettings": {
  "FromEmail": "",
  "Username": "",
  "Password": ""
}
```

Các giá trị này sẽ được override bởi environment variables trong production.
