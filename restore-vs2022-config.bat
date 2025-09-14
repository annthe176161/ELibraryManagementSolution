@echo off
echo Restoring Visual Studio 2022 configuration...
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.Development.json') -replace '\"BaseUrl\": \"https://localhost:[0-9]+\"', '\"BaseUrl\": \"https://localhost:7125\"' | Set-Content 'ELibraryManagement.Web\appsettings.Development.json'"
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.Development.json') -replace '\"BaseUrlHttp\": \".*\"', '\"BaseUrlHttp\": \"https://localhost:7125\"' | Set-Content 'ELibraryManagement.Web\appsettings.Development.json'"
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.json') -replace '\"BaseUrl\": \"https://localhost:[0-9]+\"', '\"BaseUrl\": \"https://localhost:7125\"' | Set-Content 'ELibraryManagement.Web\appsettings.json'"
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.json') -replace '\"BaseUrlHttp\": \".*\"', '\"BaseUrlHttp\": \"https://localhost:7125\"' | Set-Content 'ELibraryManagement.Web\appsettings.json'"
echo Configuration restored for Visual Studio 2022:
echo - API Server: https://localhost:7125
echo - Web App: https://localhost:7208
pause
