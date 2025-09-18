-- Script to reset AvailableQuantity for all books
-- This script will set AvailableQuantity = Quantity for all books that are not currently borrowed

USE [ELibraryManagement];

-- First, let's see current state
SELECT 
    b.Id,
    b.Title,
    b.Quantity,
    b.AvailableQuantity,
    COUNT(br.Id) as CurrentlyBorrowed
FROM Books b
LEFT JOIN BorrowRecords br ON b.Id = br.BookId AND br.Status = 1 -- 1 = Borrowed
WHERE b.IsDeleted = 0
GROUP BY b.Id, b.Title, b.Quantity, b.AvailableQuantity
ORDER BY b.Id;

-- Reset AvailableQuantity to correct values
UPDATE b
SET AvailableQuantity = b.Quantity - ISNULL(borrowed.BorrowedCount, 0)
FROM Books b
LEFT JOIN (
    SELECT BookId, COUNT(*) as BorrowedCount
    FROM BorrowRecords 
    WHERE Status = 1 -- Only count actually borrowed books (not requested)
    GROUP BY BookId
) borrowed ON b.Id = borrowed.BookId
WHERE b.IsDeleted = 0;

-- Show results after update
SELECT 
    b.Id,
    b.Title,
    b.Quantity,
    b.AvailableQuantity,
    COUNT(br.Id) as CurrentlyBorrowed
FROM Books b
LEFT JOIN BorrowRecords br ON b.Id = br.BookId AND br.Status = 1 -- 1 = Borrowed
WHERE b.IsDeleted = 0
GROUP BY b.Id, b.Title, b.Quantity, b.AvailableQuantity
ORDER BY b.Id;