# Hệ Thống Nhắc Nhở Trả Sách Tự Động

## Tổng Quan

Hệ thống tự động gửi email nhắc nhở sinh viên khi sách sắp hết hạn trả. Sinh viên có thể gia hạn sách trực tiếp từ email hoặc trang web (tối đa 2 lần).

## Tính Năng Chính

### 1. 🤖 Gửi Email Tự Động

- **Thời gian chạy**: Mỗi 24 giờ (có thể cấu hình)
- **Điều kiện gửi**: Sách sắp hết hạn trong 3 ngày, 1 ngày, hoặc hôm nay
- **Tránh spam**: Mỗi ngày chỉ gửi 1 email cho mỗi sách
- **Thông tin email**:
  - Tên sách, hạn trả, số ngày còn lại
  - Thông báo có thể gia hạn hay không
  - Link trực tiếp đến trang quản lý sách mượn
  - Cảnh báo về phí phạt

### 2. 📧 Email Template Đẹp

- **Responsive design** phù hợp mọi thiết bị
- **Màu sắc theo mức độ khẩn cấp**:
  - 🟡 Vàng: Còn 2-3 ngày
  - 🔴 Đỏ: Còn 0-1 ngày (khẩn cấp)
- **Thông tin rõ ràng** về sách và hạn trả
- **Call-to-action buttons** để gia hạn hoặc xem danh sách

### 3. ⚙️ API Endpoints

#### Admin APIs:

```http
# Xem sách sắp hết hạn
GET /api/borrow/admin/due-soon?days=7

# Gửi nhắc nhở thủ công
POST /api/borrow/admin/{id}/send-reminder
```

#### User APIs:

```http
# Gia hạn sách (tối đa 2 lần)
POST /api/borrow/{id}/extend
{
    "reason": "Lý do gia hạn"
}
```

### 4. 🔒 Bảo Mật & Quyền Hạn

- **Admin**: Có thể xem tất cả sách sắp hết hạn, gửi nhắc nhở thủ công
- **User**: Chỉ có thể gia hạn sách của mình
- **JWT Authentication** bắt buộc cho tất cả endpoints

## Cách Hoạt Động

### Background Service

```csharp
BookDueReminderService
├── Chạy mỗi 24 giờ
├── Quét sách sắp hết hạn (1-3 ngày)
├── Gửi email nhắc nhở
└── Ghi log vào Notes của BorrowRecord
```

### Quy Trình Gửi Email

1. **Kiểm tra điều kiện**: Sách đang mượn, chưa trả, sắp hết hạn
2. **Tránh duplicate**: Kiểm tra đã gửi email trong ngày chưa
3. **Tạo nội dung**: Template động theo thông tin sách và user
4. **Gửi email**: Qua SMTP đã cấu hình
5. **Ghi log**: Cập nhật Notes để tracking

### Template Email Động

```csharp
- Màu sắc: Vàng (2-3 ngày) vs Đỏ (0-1 ngày)
- Nội dung gia hạn:
  * Có thể gia hạn: Hiển thị button "Gia hạn ngay"
  * Không thể: Cảnh báo đã đạt giới hạn
- Links: Trực tiếp đến trang quản lý sách mượn
```

## Cấu Hình

### 1. Email Settings (appsettings.json)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@elibrary.com",
    "FromName": "ELibrary System",
    "EnableSsl": "true"
  }
}
```

### 2. Background Service (Program.cs)

```csharp
builder.Services.AddHostedService<BookDueReminderService>();
```

### 3. Tùy Chỉnh Thời Gian

Trong `BookDueReminderService.cs`:

```csharp
private readonly TimeSpan _period = TimeSpan.FromHours(24); // Thay đổi theo nhu cầu
```

## Testing

### 1. Test Background Service

```bash
# Khởi động API
dotnet run --project ELibraryManagement.Api

# Theo dõi logs
tail -f logs/application.log
```

### 2. Test Manual Reminder

```http
# Sử dụng file test-book-reminders.http
POST https://localhost:7001/api/borrow/admin/1/send-reminder
Authorization: Bearer {admin_token}
```

### 3. Test Extension

```http
POST https://localhost:7001/api/borrow/1/extend
Authorization: Bearer {user_token}
Content-Type: application/json

{
    "reason": "Cần thêm thời gian nghiên cứu"
}
```

## Logs & Monitoring

### Logs Quan Trọng

- ✅ **Success**: "Sent due reminder email to {email} for book: {bookTitle}"
- ⚠️ **Warning**: "Failed to send due reminder email"
- ❌ **Error**: "Error sending reminder email for borrow record {borrowId}"
- 📊 **Info**: "Processed {count} due reminders"

### Kiểm Tra Trong Database

```sql
-- Xem logs gửi email trong Notes
SELECT Id, Notes, UpdatedAt
FROM BorrowRecords
WHERE Notes LIKE '%REMINDER_%'

-- Sách sắp hết hạn
SELECT u.Email, b.Title, br.DueDate, br.ExtensionCount
FROM BorrowRecords br
JOIN Users u ON br.UserId = u.Id
JOIN Books b ON br.BookId = b.Id
WHERE br.Status = 'Borrowed'
  AND br.ReturnDate IS NULL
  AND br.DueDate BETWEEN GETDATE() AND DATEADD(day, 3, GETDATE())
```

## Troubleshooting

### Lỗi Thường Gặp

1. **Email không gửi được**

   - Kiểm tra SMTP settings
   - Kiểm tra App Password cho Gmail
   - Xem logs cho chi tiết lỗi

2. **Background Service không chạy**

   - Kiểm tra service đã được đăng ký trong Program.cs
   - Xem Application Insights hoặc logs

3. **Gia hạn không được**
   - Kiểm tra số lần gia hạn hiện tại (max 2)
   - Kiểm tra sách có quá hạn không
   - Xác nhận user có quyền gia hạn sách này

### Debug Commands

```bash
# Xem tất cả services đang chạy
dotnet run --project ELibraryManagement.Api --verbosity detailed

# Test email riêng lẻ
curl -X POST "https://localhost:7001/api/borrow/admin/1/send-reminder" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json"
```

## Future Enhancements

### Có Thể Thêm

- 📱 **SMS notifications** bổ sung email
- 🔔 **Push notifications** cho mobile app
- 📊 **Dashboard** theo dõi tỷ lệ trả sách đúng hạn
- ⏰ **Customize reminder schedule** cho từng user
- 🎯 **Smart reminders** dựa trên lịch sử user

### Cải Tiến Email

- **Multilingual support** (Tiếng Việt/English)
- **Email templates** cho các sự kiện khác
- **Unsubscribe option** cho users
- **Rich content** với hình ảnh bìa sách
