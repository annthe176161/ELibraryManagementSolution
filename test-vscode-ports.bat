@echo off
echo =========================================
echo TESTING VS CODE PORTS
echo =========================================
echo Web App: http://localhost:5224
echo API: http://localhost:5293
echo =========================================
echo.

REM Tạm thời thay đổi config để sử dụng HTTP ports
echo Switching to HTTP configuration...
powershell -Command "(Get-Content 'ELibraryManagement.Web\appsettings.Development.json') -replace '\"BaseUrl\": \"https://localhost:7125\"', '\"BaseUrl\": \"http://localhost:5293\"' | Set-Content 'ELibraryManagement.Web\appsettings.Development.json'"

echo Starting API on HTTP port 5293...
start cmd /k "cd /d ELibraryManagement.Api && dotnet run --launch-profile http"

timeout /t 3

echo Starting Web App on HTTP port 5224...
start cmd /k "cd /d ELibraryManagement.Web && dotnet run --launch-profile http"

echo.
echo Both applications are starting...
echo Please wait for both to fully load, then test at:
echo http://localhost:5224
echo.
echo IMPORTANT: After testing, run restore-vs2022-config.bat to restore VS2022 settings
echo.
pause
