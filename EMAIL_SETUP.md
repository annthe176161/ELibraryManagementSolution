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
