@echo off
echo =========================================
echo TESTING VISUAL STUDIO 2022 PORTS
echo =========================================
echo Web App: https://localhost:7208
echo API: https://localhost:7125
echo =========================================
echo.
echo Starting API on HTTPS port 7125...
start cmd /k "cd /d ELibraryManagement.Api && dotnet run --launch-profile https"

timeout /t 3

echo Starting Web App on HTTPS port 7208...
start cmd /k "cd /d ELibraryManagement.Web && dotnet run --launch-profile https"

echo.
echo Both applications are starting...
echo Please wait for both to fully load, then test at:
echo https://localhost:7208
echo.
pause
