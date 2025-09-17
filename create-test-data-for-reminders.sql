-- Script to create test data for book due reminder system
-- Run this in SQL Server Management Studio or similar tool

-- First, let's check existing data
SELECT TOP 5 * FROM AspNetUsers WHERE Email LIKE '%@%';
SELECT TOP 5 * FROM Books WHERE IsDeleted = 0;
SELECT TOP 5 * FROM BorrowRecords;

-- Insert test borrow records with due dates in 1-3 days for testing email reminders
DECLARE @UserId NVARCHAR(450);
DECLARE @BookId INT;

-- Get a test user (you can replace with actual user ID)
SELECT TOP 1 @UserId = Id FROM AspNetUsers WHERE Email LIKE '%@%';

-- Get a test book (you can replace with actual book ID)  
SELECT TOP 1 @BookId = Id FROM Books WHERE IsDeleted = 0 AND AvailableQuantity > 0;

-- Create test borrow records with different due dates for testing
INSERT INTO BorrowRecords (UserId, BookId, BorrowDate, DueDate, Status, Notes, ExtensionCount, ConfirmedDate, CreatedAt)
VALUES 
    -- Book due tomorrow (1 day left)
    (@UserId, @BookId, GETDATE(), DATEADD(day, 1, GETDATE()), 1, 'Test borrow - due tomorrow', 0, GETDATE(), GETDATE()),
    
    -- Book due in 2 days
    (@UserId, @BookId, GETDATE(), DATEADD(day, 2, GETDATE()), 1, 'Test borrow - due in 2 days', 0, GETDATE(), GETDATE()),
    
    -- Book due in 3 days
    (@UserId, @BookId, GETDATE(), DATEADD(day, 3, GETDATE()), 1, 'Test borrow - due in 3 days', 1, GETDATE(), GETDATE()),
    
    -- Book due today (urgent)
    (@UserId, @BookId, GETDATE(), CAST(GETDATE() AS DATE), 1, 'Test borrow - due today', 0, GETDATE(), GETDATE());

-- Check the inserted test data
SELECT 
    br.Id,
    u.Email,
    b.Title,
    br.BorrowDate,
    br.DueDate,
    DATEDIFF(day, GETDATE(), br.DueDate) as DaysLeft,
    br.Status,
    br.ExtensionCount,
    br.Notes
FROM BorrowRecords br
JOIN AspNetUsers u ON br.UserId = u.Id  
JOIN Books b ON br.BookId = b.Id
WHERE br.Status = 1 -- Borrowed status
    AND br.DueDate >= CAST(GETDATE() AS DATE)
    AND br.DueDate <= DATEADD(day, 3, GETDATE())
ORDER BY br.DueDate;

-- Update user email if needed (replace with your test email)
-- UPDATE AspNetUsers SET Email = 'your-test-email@gmail.com', NormalizedEmail = 'YOUR-TEST-EMAIL@GMAIL.COM' 
-- WHERE Id = @UserId;