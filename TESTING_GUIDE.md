# Hướng Dẫn Test Hệ Thống Nhắc Nhở Trả Sách

## 🎯 Cách Test Email Nhắc Nhở

### Bước 1: Cấu Hình Email Settings

Cập nhật `appsettings.json` trong `ELibraryManagement.Api`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "EnableSsl": "true",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "ELibrary System"
  }
}
```

### Bước 2: Tạo Test Data

1. Chạy file `create-test-data-for-reminders.sql` trong SQL Server
2. Thay đổi email trong script thành email thật của bạn để nhận email test

### Bước 3: Test Manual Reminder

Sử dụng API endpoint để test:

```http
POST https://localhost:7125/api/borrow/admin/trigger-reminders
Authorization: Bearer {admin-token}
Content-Type: application/json
```

### Bước 4: Test Automatic Background Service

Background service chạy mỗi 24 giờ, để test ngay:

1. Restart API server
2. Background service sẽ chạy ngay khi startup
3. Kiểm tra console logs

### Bước 5: Test Manual Reminder từ Admin Panel

1. Vào `https://localhost:7208/Admin/Borrows`
2. Tìm borrow record có trạng thái "Đang mượn"
3. Click nút "Gửi nhắc nhở" (nếu có)

## 🔧 Fix Admin Approval Issue

### Vấn đề đã được sửa:

- ✅ Gộp status update và notes update thành 1 API call
- ✅ Thêm logging chi tiết
- ✅ Sửa interface và implementation

### Test lại:

1. Vào `https://localhost:7208/Admin/Borrows`
2. Tìm borrow record có trạng thái "Chờ duyệt"
3. Click "Phê duyệt"
4. Kiểm tra console logs và database

## 📧 Kiểm Tra Email

- Gmail: Kiểm tra cả Inbox và Spam folder
- Outlook: Kiểm tra Junk folder
- Console: Xem logs để biết email có được gửi không

## 🚨 Troubleshooting

### Email không gửi được:

1. Kiểm tra EmailSettings trong appsettings.json
2. Đảm bảo Gmail App Password được tạo đúng
3. Kiểm tra console logs cho error messages

### Admin approval không hoạt động:

1. Mở F12 Developer Tools
2. Kiểm tra Console tab cho JavaScript errors
3. Kiểm tra Network tab cho HTTP requests
4. Xem server logs trong terminal

### Background service không chạy:

1. Kiểm tra console logs khi startup
2. Đảm bảo có test data với due date phù hợp
3. Service chỉ gửi email cho sách due trong 1-3 ngày

## 📊 API Endpoints Mới

### Trigger Manual Reminders

```http
POST /api/borrow/admin/trigger-reminders
```

### Get Books Due Soon

```http
GET /api/borrow/admin/due-soon?days=7
```

### Send Individual Reminder

```http
POST /api/borrow/admin/{id}/send-reminder
```
