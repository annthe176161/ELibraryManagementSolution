# PowerShell script to reset database quickly
param(
    [string]$ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ELibraryManagement;Trusted_Connection=true;MultipleActiveResultSets=true"
)

Write-Host "Resetting AvailableQuantity for all books..." -ForegroundColor Green

$sqlCommand = @"
UPDATE Books 
SET AvailableQuantity = Quantity - (
    SELECT COUNT(*) 
    FROM BorrowRecords 
    WHERE BookId = Books.Id AND Status = 1
)
WHERE IsDeleted = 0;

SELECT 
    Id, Title, Quantity, AvailableQuantity,
    (SELECT COUNT(*) FROM BorrowRecords WHERE BookId = Books.Id AND Status = 1) as ActuallyBorrowed
FROM Books 
WHERE IsDeleted = 0
ORDER BY Id;
"@

try {
    # Execute SQL
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlCommand, $connection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset)
    
    Write-Host "Update completed successfully!" -ForegroundColor Green
    Write-Host "Current book status:" -ForegroundColor Yellow
    
    $dataset.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Done! You can now refresh the web page." -ForegroundColor Green