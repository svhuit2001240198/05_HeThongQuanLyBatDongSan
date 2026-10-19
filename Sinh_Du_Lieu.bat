@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0DoAn_NoSQL_Nhom5"
set "ASPNETCORE_ENVIRONMENT=Development"
echo Sinh lại 10.000 Users và 18 BĐS demo bằng C#.
echo Lệnh này ghi lại file dữ liệu mẫu, không xóa dữ liệu trong MongoDB.
dotnet run --no-launch-profile -- --generate-data 10000
pause
