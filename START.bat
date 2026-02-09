@echo off
echo.
echo ========================================
echo   Starting Path AI Student Monitoring
echo ========================================
echo.
echo Stopping any existing instances...
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM Path.exe >nul 2>&1
timeout /t 2 >nul

echo Starting application...
cd /d "%~dp0"
start http://localhost:5000
dotnet run

pause
