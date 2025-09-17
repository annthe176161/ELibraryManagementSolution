# Hệ Thống Quản Lý Phạt (Fine Management System)

## Tổng quan

Hệ thống quản lý phạt đã được tích hợp vào ELibrary Management System để giúp admin theo dõi và xử lý các khoản phạt của sinh viên.

## Tính năng chính

### 1. Dashboard Thống kê

- **Tổng quan phạt**: Hiển thị số lượng và tổng tiền phạt theo trạng thái
- **Phân loại trạng thái**:
  - Pending: Chưa thanh toán
  - Paid: Đã thanh toán
  - Waived: Đã miễn phạt
  - Overdue: Quá hạn thanh toán

### 2. Quản lý phạt

- **Xem danh sách**: Hiển thị tất cả phạt với filter và pagination
- **Tìm kiếm**: Tìm theo tên, email sinh viên hoặc lý do phạt
- **Filter**: Lọc theo trạng thái, khoảng thời gian, mức tiền
- **Tạo phạt mới**: Form tạo phạt với validation đầy đủ
- **Xem chi tiết**: Modal hiển thị thông tin chi tiết và lịch sử

### 3. Xử lý phạt

- **Đánh dấu đã thanh toán**: Cập nhật trạng thái và ghi nhận thanh toán
- **Miễn phạt**: Waive phạt với lý do cụ thể
- **Gửi nhắc nhở**: Tự động track số lần nhắc và ngày nhắc cuối
- **Audit trail**: Lưu toàn bộ lịch sử thao tác

## Cấu trúc mã nguồn

### API Layer (`ELibraryManagement.Api`)

```
Controllers/
├── FineController.cs       # RESTful API endpoints
Models/
├── Fine.cs                 # Model chính
├── FineActionHistory.cs    # Model lịch sử hành động
DTOs/
├── FineDto.cs             # Data transfer objects
```

### Web Layer (`ELibraryManagement.Web`)

```
Controllers/
├── AdminController.cs      # Admin actions for fine management
Services/
├── FineApiService.cs      # HTTP client service
Models/
├── FineViewModels.cs      # ViewModels with validation
Views/Admin/
├── Fines.cshtml           # Main listing page
├── CreateFine.cshtml      # Create fine form
├── _FineDetailModal.cshtml # Detail modal partial
```

## API Endpoints

### Fine Management

```
GET    /api/fine                    # Lấy danh sách phạt (có pagination, filter)
GET    /api/fine/{id}              # Lấy chi tiết phạt
GET    /api/fine/statistics        # Thống kê tổng quan
POST   /api/fine                   # Tạo phạt mới
PUT    /api/fine/{id}              # Cập nhật phạt
POST   /api/fine/{id}/mark-paid    # Đánh dấu đã thanh toán
POST   /api/fine/{id}/waive        # Miễn phạt
POST   /api/fine/{id}/send-reminder # Gửi nhắc nhở
```

## Database Schema

### Bảng Fines

```sql
- Id (int, PK)
- UserId (nvarchar, FK -> AspNetUsers)
- BorrowRecordId (int, FK -> BorrowRecords, nullable)
- Amount (decimal)
- Reason (nvarchar)
- Description (nvarchar)
- Status (int) -- 0:Pending, 1:Paid, 2:Waived
- FineDate (datetime)
- DueDate (datetime, nullable)
- PaidDate (datetime, nullable)
- ReminderCount (int)
- LastReminderDate (datetime, nullable)
- CreatedAt, UpdatedAt
```

### Bảng FineActionHistories

```sql
- Id (int, PK)
- FineId (int, FK -> Fines)
- UserId (nvarchar, FK -> AspNetUsers)
- ActionType (int) -- 0:ReminderSent, 1:PaymentReceived, 2:Escalated, 3:Reduced, 4:Increased, 5:FineWaived
- Description (nvarchar)
- Notes (nvarchar, nullable)
- Amount (decimal, nullable)
- ActionDate (datetime)
- CreatedAt
```

## Cách sử dụng

### 1. Cài đặt dữ liệu test

```sql
-- Chạy script tạo dữ liệu mẫu
sqlcmd -S localhost -d ELibraryManagement -i create-test-data-for-fines.sql
```

### 2. Truy cập trang quản lý

1. Đăng nhập với tài khoản admin
2. Vào menu **Admin** → **Fine Management**
3. Xem dashboard và danh sách phạt

### 3. Thao tác cơ bản

- **Tạo phạt mới**: Click "Create New Fine"
- **Xem chi tiết**: Click "View" ở hàng phạt
- **Đánh dấu đã thanh toán**: Click "Mark as Paid"
- **Miễn phạt**: Click "Waive Fine"
- **Tìm kiếm**: Dùng search box
- **Filter**: Chọn status, date range

## Testing

### Test cases cơ bản

1. **Dashboard**: Kiểm tra thống kê hiển thị đúng
2. **Listing**: Test pagination, search, filter
3. **Create**: Validation form tạo phạt
4. **Payment**: Đánh dấu thanh toán và kiểm tra audit trail
5. **Waive**: Miễn phạt với lý do
6. **Overdue**: Kiểm tra phạt quá hạn hiển thị đúng

### HTTP Test files

Sử dụng các file `.http` để test API:

```
test-fine-management.http    # Test API endpoints
```

## Lưu ý quan trọng

### Security

- Chỉ admin mới có quyền truy cập
- Validate đầy đủ input data
- Audit trail cho mọi thao tác

### Performance

- Pagination để xử lý lượng data lớn
- Index trên các cột tìm kiếm thường xuyên
- Cache statistics khi cần

### Business Rules

- Không được xóa phạt (chỉ waive)
- Phạt quá hạn tự động đánh dấu overdue
- Lưu đầy đủ lịch sử thao tác
- Email nhắc nhở tự động (cần config SMTP)

## Mở rộng trong tương lai

- Email notifications tự động
- Payment gateway integration
- Mobile responsive optimization
- Advanced reporting & analytics
- Bulk operations
- Fine templates cho các loại phạt thường gặp
