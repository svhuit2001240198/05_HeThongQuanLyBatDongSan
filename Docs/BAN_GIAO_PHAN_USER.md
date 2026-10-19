# Phần việc User và các điểm ghép nhóm

Gói bổ sung trực tiếp trên project C# của nhóm, target `net10.0`. Collection mặc định là **Users**, không phải `User` viết số ít. File cấu hình cho phép đổi tên nếu cả nhóm thống nhất.

## Mở file nào trước

| Mục đích | File |
|---|---|
| Chạy và xử lý lỗi | `BAT_DAU_O_DAY.txt`, `HUONG_DAN_CHAY.md`, `Chay_Web.bat` |
| Nội dung báo cáo phần User | `Docs/BaoCao_User_Nhom5.docx` |
| Sơ đồ liên kết | `Docs/SoDo_Users_BDS.png` / `.svg` / `.mmd` |
| Schema và quy tắc đầu vào | `Models/User.cs`, `ViewModels/AccountViewModels.cs` |
| Nghiệp vụ User | `Services/UserService.cs` |
| Đăng ký, đăng nhập, hồ sơ | `Controllers/AccountController.cs`, `Views/Account/` |
| Môi giới / quản trị | `BrokersController.cs`, `AdminController.cs` và Views tương ứng |
| Giao diện chung | `Views/Shared/`, `wwwroot/css/account.css`, `wwwroot/js/account.js` |
| Tạo và nạp dữ liệu | `Services/MockDataGenerator.cs`, `Services/SeedService.cs` |
| Truy vấn và index | `Queries/UserQueries.cs`, `Queries/Users.mongosh.js`, `Infrastructure/MongoContext.cs` |
| Redis | `Services/RedisHistoryService.cs` |

Các đường dẫn source ở bảng trên tính từ thư mục con `DoAn_NoSQL_Nhom5/`.

## Ghép với bạn làm BĐS

1. Gửi `Data/mock/MaUser_VaiTro.csv` cho hai thành viên còn lại. Dùng mã có vai trò `nguoi_ban` làm chủ sở hữu, `moi_gioi` làm môi giới theo thiết kế của nhóm.
2. Thống nhất MongoDB database `DoAn_NoSQL` và tên collection `Users`, `BDS`. Đổi trong cấu hình nếu nhóm đã có quy ước khác.
3. BĐS tham chiếu `Users.MaUser` ở `MaChuSoHuu`, `MaMoiGioi`, `LichSuGia[].MaNguoiThayDoi`. Tên người dùng và MongoDB `_id` không dùng làm mã nối.
4. Dữ liệu BĐS phải có giá trị MaBDS duy nhất. Tin đã lưu/đánh giá/thông báo nối về `BDS.MaBDS`.
5. `PropertyService` và Home chỉ đọc BĐS để thử phần User. Ghép controller nghiệp vụ giao dịch/sửa giá của bạn phụ trách BĐS vào project này hoặc mang phần User sang project chung; kiểm tra cấu hình và DI trong `Program.cs` khi ghép.

## Nội dung còn thuộc phần khác của nhóm

Gói có **10.000 User** và **18 BĐS demo**. Nhóm vẫn cần bộ dữ liệu BĐS đủ số lượng theo đề, giao dịch/lịch sử giá, địa lý/bản đồ và sơ đồ tổng thể. Lịch hẹn/nhu cầu mua chưa có model trong source gốc; trường MaLichHen được giữ tùy chọn, chưa có chức năng tạo lịch hẹn. Giao diện hiện dùng avatar chữ cái, chưa có upload ảnh.

## Trước khi nộp

- Build và chạy project trên máy có .NET 10 SDK, MongoDB, Redis; làm theo hướng dẫn.
- Chạy `Tools/smoke_test.py` sau khi web đã mở; script tạo một User test riêng.
- Đo `explain("executionStats")` trên dữ liệu thật của máy rồi đưa kết quả vào báo cáo; không dùng số đo giả định.
- Kiểm tra giao diện desktop/mobile trên trình duyệt. Bản HTML tĩnh chỉ giúp xem bố cục, không có database.
- Ghép phần User trong báo cáo Word vào báo cáo chung; bổ sung tên thành viên, ảnh màn hình chạy thật và kết quả đo thực tế.

Kiểm tra đã làm khi chuẩn bị gói: dữ liệu mock, cấu hình JSON/XML, tài nguyên giao diện, cú pháp Python/JavaScript, rà soát C#/Razor và render báo cáo Word. **Chưa xác nhận build, chạy MongoDB/Redis hoặc các luồng web thật trong môi trường chuẩn bị.**
