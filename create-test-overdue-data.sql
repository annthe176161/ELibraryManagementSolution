-- Script để tạo dữ liệu test cho chức năng xử lý sách quá hạn tự động
-- Chạy script này để tạo borrow records với DueDate trong quá khứ
-- Mức phạt: 5,000 VNĐ/ngày (cố định)

USE ELibraryDb;

-- Tạo một số borrow records quá hạn để test
DECLARE @UserId NVARCHAR(450);
DECLARE @BookId INT;

-- Lấy user đầu tiên (không phải admin)
SELECT TOP 1 @UserId = Id FROM AspNetUsers WHERE Id NOT IN (
    SELECT UserId FROM AspNetUserRoles 
    WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
);

-- Lấy book đầu tiên có sẵn
SELECT TOP 1 @BookId = Id FROM Books WHERE AvailableQuantity > 0 AND IsDeleted = 0;

-- Tạo borrow record quá hạn 1 ngày (phạt: 1 x 5,000 = 5,000 VNĐ)
INSERT INTO BorrowRecords (UserId, BookId, BorrowDate, DueDate, Status, Notes, CreatedAt)
VALUES (
    @UserId, 
    @BookId, 
    DATEADD(day, -8, GETDATE()), -- Mượn 8 ngày trước
    DATEADD(day, -1, GETDATE()), -- Hạn trả 1 ngày trước
    1, -- BorrowStatus.Borrowed = 1
    'Test data - Sách quá hạn 1 ngày (phạt: 5,000 VNĐ)',
    DATEADD(day, -8, GETDATE())
);

-- Lấy book thứ 2
SELECT @BookId = Id FROM Books WHERE Id > @BookId AND AvailableQuantity > 0 AND IsDeleted = 0;

-- Tạo borrow record quá hạn 5 ngày (phạt: 5 x 5,000 = 25,000 VNĐ)
INSERT INTO BorrowRecords (UserId, BookId, BorrowDate, DueDate, Status, Notes, CreatedAt)
VALUES (
    @UserId, 
    @BookId, 
    DATEADD(day, -12, GETDATE()), -- Mượn 12 ngày trước
    DATEADD(day, -5, GETDATE()), -- Hạn trả 5 ngày trước
    1, -- BorrowStatus.Borrowed = 1
    'Test data - Sách quá hạn 5 ngày (phạt: 25,000 VNĐ)',
    DATEADD(day, -12, GETDATE())
);

-- Lấy book thứ 3
SELECT @BookId = Id FROM Books WHERE Id > @BookId AND AvailableQuantity > 0 AND IsDeleted = 0;

-- Tạo borrow record quá hạn 10 ngày (phạt: 10 x 5,000 = 50,000 VNĐ)
INSERT INTO BorrowRecords (UserId, BookId, BorrowDate, DueDate, Status, Notes, CreatedAt)
VALUES (
    @UserId, 
    @BookId, 
    DATEADD(day, -25, GETDATE()), -- Mượn 25 ngày trước
    DATEADD(day, -10, GETDATE()), -- Hạn trả 10 ngày trước
    1, -- BorrowStatus.Borrowed = 1
    'Test data - Sách quá hạn 10 ngày (phạt: 50,000 VNĐ)',
    DATEADD(day, -25, GETDATE())
);

-- Cập nhật AvailableQuantity của các sách (giảm đi vì đã được mượn)
UPDATE Books 
SET AvailableQuantity = AvailableQuantity - 1 
WHERE Id IN (
    SELECT DISTINCT BookId FROM BorrowRecords 
    WHERE Notes LIKE 'Test data - Sách quá hạn%'
);

-- Kiểm tra dữ liệu vừa tạo
SELECT 
    br.Id as BorrowRecordId,
    u.FirstName + ' ' + u.LastName as UserName,
    b.Title as BookTitle,
    br.BorrowDate,
    br.DueDate,
    br.Status,
    DATEDIFF(day, br.DueDate, GETDATE()) as OverdueDays,
    br.Notes
FROM BorrowRecords br
JOIN AspNetUsers u ON br.UserId = u.Id
JOIN Books b ON br.BookId = b.Id
WHERE br.Notes LIKE 'Test data - Sách quá hạn%'
ORDER BY br.DueDate;

PRINT 'Đã tạo ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' borrow records quá hạn để test';