# Hướng dẫn sửa lỗi Email Reminder System

## Vấn đề đã phát hiện:

1. **Cấu hình email thiếu thông tin**: Trong file `appsettings.Development.json`, các trường `FromEmail`, `Username`, và `Password` đang để trống.

2. **Logic gửi email chưa bao gồm trường hợp "còn 2 ngày"**: Service chỉ gửi email khi còn 3, 1, hoặc 0 ngày, mà không bao gồm 2 ngày.

## Các thay đổi đã thực hiện:

### 1. Cập nhật BookDueReminderService:

- Thêm case gửi email khi còn 2 ngày
- Thêm logging chi tiết để debug
- Cải thiện logic tính toán ngày

### 2. Cập nhật EmailService:

- Thêm validation cho cấu hình email
- Thêm logging chi tiết để debug
- Ngăn chặn gửi email khi thiếu thông tin cấu hình

## Hướng dẫn khắc phục:

### Bước 1: Cấu hình Email Settings

Mở file `ELibraryManagement.Api/appsettings.Development.json` và cập nhật thông tin email:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "your-email@gmail.com",
    "FromName": "ELibrary Management System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

**Lưu ý quan trọng về Gmail:**

- Sử dụng **App Password** thay vì mật khẩu thường
- Bật **2-Factor Authentication** cho tài khoản Gmail
- Tạo App Password tại: https://myaccount.google.com/apppasswords

### Bước 2: Tạo App Password cho Gmail

1. Đăng nhập vào tài khoản Google
2. Vào **Google Account Settings** > **Security**
3. Bật **2-Step Verification** (nếu chưa bật)
4. Vào **App passwords**
5. Chọn **Mail** và **Other** (nhập tên ứng dụng)
6. Copy mật khẩu 16 ký tự vào trường `Password`

### Bước 3: Test chức năng

1. **Restart API application** để load cấu hình mới
2. **Kiểm tra logs** trong console để xem:
   - Service có chạy không?
   - Có tìm thấy records nào cần gửi email không?
   - Có lỗi khi gửi email không?

### Bước 4: Kiểm tra logs

Sau khi restart, bạn sẽ thấy các log như:

```
Book due reminder service executed at: [time]
Vietnam time now: [time], today: [date]
Found X borrows nearing due date
Processing borrow X: DueDate=[date], DueInVietnam=[date], DaysLeft=X
Attempting to send email to [email] with subject: [subject]
Email sent successfully to [email]
```

### Bước 5: Tạo test case

Để test ngay lập tức, có thể:

1. **Thay đổi thời gian check** từ 1 phút xuống 10 giây (trong development):

```csharp
private readonly TimeSpan _period = TimeSpan.FromSeconds(10); // Test mode
```

2. **Kiểm tra database** xem có records nào thỏa mãn điều kiện không

## Troubleshooting:

### Nếu vẫn không gửi được email:

1. **Kiểm tra firewall/antivirus** có block SMTP không
2. **Thử email provider khác** (Outlook, Yahoo)
3. **Kiểm tra logs** xem có exception gì không
4. **Test gửi email thủ công** qua API endpoint

### Nếu không tìm thấy records:

1. **Kiểm tra timezone** có đúng không
2. **Kiểm tra dữ liệu** trong database
3. **Xem logs** để debug logic tính toán ngày

## Email Providers khác:

### Outlook/Hotmail:

```json
{
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587
}
```

### Yahoo:

```json
{
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587
}
```

## Kết luận:

Sau khi thực hiện các bước trên, chức năng gửi email nhắc nhở sẽ hoạt động bình thường cho tất cả các trường hợp: còn 3, 2, 1, và 0 ngày trước hạn trả sách.
