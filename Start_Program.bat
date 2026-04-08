@echo off
echo ===========================
echo Clean and Build
echo ===========================
dotnet clean
dotnet build
echo ===========================
echo Starter Backend
echo ===========================
start "Backend" cmd /k "cd HelpDeskGF2 && dotnet run --project HelpDeskGF2.csproj
echo .
echo ===========================
echo Starter Frontend
echo ===========================
start "Frontend" cmd /k "cd HelpDeskFrontend && dotnet run --project HelpDeskFrontend.csproj
echo.
pause
