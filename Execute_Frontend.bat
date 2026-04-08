@echo off
echo Starter HelpDeskFrontend

cd ./HelpDeskFrontend
dotnet clean
dotnet build
dotnet run --project HelpDeskFrontend.csproj

pause