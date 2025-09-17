-- Script tạo dữ liệu test cho Fine Management System
-- Chạy script này sau khi đã có dữ liệu user và borrow records

USE [ELibraryManagement]
GO

-- 1. Tạo một số phạt mẫu cho testing
DECLARE @AdminUserId NVARCHAR(450)
DECLARE @StudentUserId NVARCHAR(450)
DECLARE @BorrowRecordId INT

-- Lấy Admin user ID (để tạo action history)
SELECT TOP 1 @AdminUserId = Id FROM AspNetUsers 
WHERE Email LIKE '%admin%' OR Id IN (
    SELECT UserId FROM AspNetUserRoles 
    WHERE RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
)

-- Lấy Student user ID
SELECT TOP 1 @StudentUserId = Id FROM AspNetUsers 
WHERE Id IN (
    SELECT UserId FROM AspNetUserRoles 
    WHERE RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'User')
)

-- Lấy một borrow record ID
SELECT TOP 1 @BorrowRecordId = Id FROM BorrowRecords 
WHERE UserId = @StudentUserId

-- Chỉ thực hiện nếu có dữ liệu cần thiết
IF @AdminUserId IS NOT NULL AND @StudentUserId IS NOT NULL
BEGIN
    -- Xóa dữ liệu cũ nếu có
    DELETE FROM FineActionHistories
    DELETE FROM Fines

    -- 1. Phạt trả sách muộn (Pending)
    INSERT INTO Fines (UserId, BorrowRecordId, Amount, Reason, Description, Status, FineDate, DueDate, CreatedAt)
    VALUES (
        @StudentUserId,
        @BorrowRecordId,
        15000.00,
        N'Trả sách muộn',
        N'Phạt tiêu chuẩn cho việc trả sách muộn 3 ngày. Mức phạt 5,000 VND/ngày.',
        0, -- Pending
        DATEADD(DAY, -5, GETDATE()),
        DATEADD(DAY, 25, GETDATE()),
        DATEADD(DAY, -5, GETDATE())
    )

    DECLARE @FineId1 INT = SCOPE_IDENTITY()

    -- 2. Phạt làm hỏng sách (Paid)
    INSERT INTO Fines (UserId, Amount, Reason, Description, Status, FineDate, PaidDate, DueDate, CreatedAt)
    VALUES (
        @StudentUserId,
        50000.00,
        N'Làm hỏng sách',
        N'Sách bị rách một số trang và bẩn. Cần sửa chữa.',
        1, -- Paid
        DATEADD(DAY, -10, GETDATE()),
        DATEADD(DAY, -2, GETDATE()),
        DATEADD(DAY, 20, GETDATE()),
        DATEADD(DAY, -10, GETDATE())
    )

    DECLARE @FineId2 INT = SCOPE_IDENTITY()

    -- 3. Phạt quá hạn (Overdue)
    INSERT INTO Fines (UserId, Amount, Reason, Description, Status, FineDate, DueDate, CreatedAt)
    VALUES (
        @StudentUserId,
        25000.00,
        N'Trả sách muộn',
        N'Phạt cho việc trả sách muộn 5 ngày.',
        0, -- Pending nhưng quá hạn
        DATEADD(DAY, -40, GETDATE()),
        DATEADD(DAY, -10, GETDATE()), -- Đã quá hạn thanh toán
        DATEADD(DAY, -40, GETDATE())
    )

    DECLARE @FineId3 INT = SCOPE_IDENTITY()

    -- 4. Phạt đã miễn (Waived)
    INSERT INTO Fines (UserId, Amount, Reason, Description, Status, FineDate, DueDate, CreatedAt)
    VALUES (
        @StudentUserId,
        10000.00,
        N'Vi phạm quy định thư viện',
        N'Sử dụng điện thoại trong khu vực yên tĩnh.',
        2, -- Waived
        DATEADD(DAY, -15, GETDATE()),
        DATEADD(DAY, 15, GETDATE()),
        DATEADD(DAY, -15, GETDATE())
    )

    DECLARE @FineId4 INT = SCOPE_IDENTITY()

    -- Thêm lịch sử hành động cho các phạt

    -- Fine 1: Pending - Có gửi nhắc nhở
    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId1,
        @AdminUserId,
        0, -- ReminderSent
        N'Tạo phạt mới: Trả sách muộn',
        15000.00,
        DATEADD(DAY, -5, GETDATE()),
        DATEADD(DAY, -5, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId1,
        @AdminUserId,
        0, -- ReminderSent
        N'Gửi nhắc nhở thanh toán phạt qua email',
        15000.00,
        DATEADD(DAY, -2, GETDATE()),
        DATEADD(DAY, -2, GETDATE())
    )

    -- Fine 2: Paid - Có lịch sử thanh toán
    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId2,
        @AdminUserId,
        0, -- ReminderSent
        N'Tạo phạt mới: Làm hỏng sách',
        50000.00,
        DATEADD(DAY, -10, GETDATE()),
        DATEADD(DAY, -10, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Notes, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId2,
        @AdminUserId,
        1, -- PaymentReceived
        N'Đánh dấu phạt đã thanh toán - Số tiền: 50,000 VND',
        N'Sinh viên đã thanh toán bằng tiền mặt tại quầy thủ thư',
        50000.00,
        DATEADD(DAY, -2, GETDATE()),
        DATEADD(DAY, -2, GETDATE())
    )

    -- Fine 3: Overdue - Có nhiều lần nhắc nhở
    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId3,
        @AdminUserId,
        0, -- ReminderSent
        N'Tạo phạt mới: Trả sách muộn',
        25000.00,
        DATEADD(DAY, -40, GETDATE()),
        DATEADD(DAY, -40, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId3,
        @AdminUserId,
        0, -- ReminderSent
        N'Gửi nhắc nhở thanh toán phạt (lần 1)',
        25000.00,
        DATEADD(DAY, -25, GETDATE()),
        DATEADD(DAY, -25, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId3,
        @AdminUserId,
        0, -- ReminderSent
        N'Gửi nhắc nhở thanh toán phạt (lần 2)',
        25000.00,
        DATEADD(DAY, -15, GETDATE()),
        DATEADD(DAY, -15, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId3,
        @AdminUserId,
        2, -- Escalated
        N'Chuyển lên cấp trên do quá hạn thanh toán',
        25000.00,
        DATEADD(DAY, -5, GETDATE()),
        DATEADD(DAY, -5, GETDATE())
    )

    -- Fine 4: Waived - Có lịch sử miễn phạt
    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId4,
        @AdminUserId,
        0, -- ReminderSent
        N'Tạo phạt mới: Vi phạm quy định thư viện',
        10000.00,
        DATEADD(DAY, -15, GETDATE()),
        DATEADD(DAY, -15, GETDATE())
    )

    INSERT INTO FineActionHistories (FineId, UserId, ActionType, Description, Notes, Amount, ActionDate, CreatedAt)
    VALUES (
        @FineId4,
        @AdminUserId,
        5, -- FineWaived
        N'Miễn phạt - Lý do: Sinh viên thành thật thừa nhận lỗi và hứa không tái phạm',
        N'Đây là lần đầu vi phạm của sinh viên, quyết định miễn phạt để tạo động lực tích cực',
        10000.00,
        DATEADD(DAY, -12, GETDATE()),
        DATEADD(DAY, -12, GETDATE())
    )

    -- Cập nhật số lần nhắc nhở
    UPDATE Fines SET ReminderCount = 2, LastReminderDate = DATEADD(DAY, -2, GETDATE()) WHERE Id = @FineId1
    UPDATE Fines SET ReminderCount = 1, LastReminderDate = DATEADD(DAY, -2, GETDATE()) WHERE Id = @FineId2
    UPDATE Fines SET ReminderCount = 3, LastReminderDate = DATEADD(DAY, -15, GETDATE()) WHERE Id = @FineId3
    UPDATE Fines SET ReminderCount = 1, LastReminderDate = DATEADD(DAY, -15, GETDATE()) WHERE Id = @FineId4

    PRINT N'Đã tạo thành công dữ liệu test cho Fine Management:'
    PRINT N'- 4 phạt với các trạng thái khác nhau'
    PRINT N'- Lịch sử hành động cho từng phạt'
    PRINT N'- Phạt có hạn thanh toán và quá hạn'
    PRINT N''
    PRINT N'Thống kê:'
    PRINT N'- Pending: 2 phạt (1 trong hạn, 1 quá hạn)'
    PRINT N'- Paid: 1 phạt'
    PRINT N'- Waived: 1 phạt'
    PRINT N'- Tổng tiền: 100,000 VND'
    PRINT N'- Đã thu: 50,000 VND'
    PRINT N'- Chưa thu: 40,000 VND (1 phạt pending 15,000 + 1 phạt overdue 25,000)'
END
ELSE
BEGIN
    PRINT N'Không tìm thấy dữ liệu cần thiết (Admin user hoặc Student user)'
    PRINT N'Vui lòng đảm bảo:'
    PRINT N'1. Có ít nhất 1 admin user'
    PRINT N'2. Có ít nhất 1 student user (role = User)'
    PRINT N'3. Có ít nhất 1 borrow record'
END

-- Kiểm tra kết quả
SELECT 
    F.Id,
    U.Email AS UserEmail,
    F.Amount,
    F.Reason,
    F.Status,
    F.FineDate,
    F.DueDate,
    F.PaidDate,
    CASE 
        WHEN F.DueDate IS NOT NULL AND F.DueDate < GETDATE() AND F.Status = 0 
        THEN N'Quá hạn'
        ELSE N'Trong hạn'
    END AS PaymentStatus
FROM Fines F
JOIN AspNetUsers U ON F.UserId = U.Id
ORDER BY F.CreatedAt DESC

-- Thống kê tổng quan
SELECT 
    N'Tổng số phạt' as Metric,
    COUNT(*) as Value
FROM Fines
UNION ALL
SELECT 
    N'Pending',
    COUNT(*)
FROM Fines WHERE Status = 0
UNION ALL
SELECT 
    N'Paid',
    COUNT(*)
FROM Fines WHERE Status = 1
UNION ALL
SELECT 
    N'Waived',
    COUNT(*)
FROM Fines WHERE Status = 2
UNION ALL
SELECT 
    N'Overdue',
    COUNT(*)
FROM Fines WHERE DueDate IS NOT NULL AND DueDate < GETDATE() AND Status = 0