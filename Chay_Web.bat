@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0DoAn_NoSQL_Nhom5"
set "ASPNETCORE_ENVIRONMENT=Development"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Chưa tìm thấy .NET SDK. Cài .NET 10 SDK rồi chạy lại.
  pause
  exit /b 1
)
echo Đang khôi phục thư viện và chuẩn bị project...
dotnet restore
if errorlevel 1 goto loi
echo Đang nạp dữ liệu mẫu. Các bản ghi có cùng mã sẽ được giữ nguyên.
dotnet run --no-launch-profile -- --seed
if errorlevel 1 goto loi
echo Mở trình duyệt tại http://localhost:5156
echo Dừng web bằng Ctrl+C.
dotnet run --no-build --no-launch-profile --urls http://localhost:5156
pause
exit /b
:loi
echo.
echo Chưa chạy được. Xem thông báo phía trên và file HUONG_DAN_CHAY.md.
echo MongoDB cần đang chạy tại localhost:27017.
pause
exit /b 1
