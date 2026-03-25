@echo off
echo Cleaning solution...
dotnet clean HelpDeskGF2.slnx

echo.
echo Building solution...
dotnet build HelpDeskGF2.slnx

echo.
echo Running project...
dotnet run --project HelpDeskGF2

pause