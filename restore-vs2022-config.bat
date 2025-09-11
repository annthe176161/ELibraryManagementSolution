@echo off
echo Restoring Visual Studio 2022 configuration...
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.Development.json') -replace '\"BaseUrl\": \"http://localhost:5293\"', '\"BaseUrl\": \"https://localhost:7125\"' | Set-Content 'ELibraryManagement.Web\appsettings.Development.json'"
echo Configuration restored for Visual Studio 2022 (HTTPS ports)
pause
