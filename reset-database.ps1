# PowerShell script to reset database and reseed data

Write-Host "=== Resetting ELibrary Database ===" -ForegroundColor Green

# Navigate to API project directory
Set-Location "ELibraryManagement.Api"

Write-Host "1. Dropping existing database..." -ForegroundColor Yellow
dotnet ef database drop --force

Write-Host "2. Applying migrations..." -ForegroundColor Yellow
dotnet ef database update

Write-Host "3. Database reset complete!" -ForegroundColor Green
Write-Host "The database has been recreated with fresh seed data." -ForegroundColor Green

# Go back to solution root
Set-Location ".."

Write-Host "=== Process Complete ===" -ForegroundColor Green
Write-Host "You can now run the application to see the fresh data." -ForegroundColor Green