// Chỉ đọc dữ liệu. Chạy từ thư mục gói:
// mongosh "mongodb://localhost:27017/DoAn_NoSQL" --file DoAn_NoSQL_Nhom5/Queries/Users.mongosh.js
// Đổi tên collection ở đây nếu nhóm đã đổi trong appsettings.json.
const demoDb = db.getSiblingDB("DoAn_NoSQL");
const users = demoDb.getCollection("Users");

print("1. Số lượng User theo vai trò");
printjson(users.aggregate([
  { $group: { _id: "$VaiTro", SoLuong: { $sum: 1 } } },
  { $sort: { _id: 1 } }
]).toArray());

print("2. Người mua gần đây; chỉ xuất trường cần thiết");
printjson(users.find({ VaiTro: "nguoi_mua" },
  { _id: 0, MaUser: 1, HoTen: 1, VaiTro: 1, NgayTao: 1 })
  .sort({ NgayTao: -1, MaUser: 1 }).limit(5).toArray());

print("3. Môi giới TP. Hồ Chí Minh có ít nhất 5 năm kinh nghiệm");
printjson(users.find({
  VaiTro: "moi_gioi", "ThongTinMoiGioi.KhuVucHoatDong": "TP. Hồ Chí Minh",
  "ThongTinMoiGioi.KinhNghiem": { $gte: 5 }
}, { _id: 0, MaUser: 1, HoTen: 1, ThongTinMoiGioi: 1 }).limit(5).toArray());

print("4. Join tin đã lưu của User demo");
printjson(users.aggregate([
  { $match: { MaUser: "USR-DEMO-BUYER" } },
  { $project: { _id: 0, MaUser: 1, BDSDaLuu: 1 } },
  { $lookup: { from: "BDS", localField: "BDSDaLuu", foreignField: "MaBDS", as: "TinDaLuu" } },
  { $project: { MaUser: 1, "TinDaLuu.MaBDS": 1, "TinDaLuu.TieuDe": 1, "TinDaLuu.Gia": 1 } }
]).toArray());

print("5. Top 10 môi giới theo điểm trung bình");
printjson(users.aggregate([
  { $match: { VaiTro: "moi_gioi", TrangThaiTaiKhoan: { $ne: "bi_khoa" } } },
  { $unwind: "$DanhGia" },
  { $group: { _id: "$MaUser", HoTen: { $first: "$HoTen" },
      DiemTrungBinh: { $avg: "$DanhGia.SoSao" }, SoDanhGia: { $sum: 1 } } },
  { $sort: { DiemTrungBinh: -1, SoDanhGia: -1, _id: 1 } }, { $limit: 10 }
]).toArray());

print("6. Số tài khoản tạo theo tháng UTC+7");
printjson(users.aggregate([
  { $group: { _id: { $dateToString: { date: "$NgayTao", format: "%Y-%m", timezone: "+07:00" } },
      SoLuong: { $sum: 1 } } }, { $sort: { _id: 1 } }
]).toArray());

print("7. Lọc phần tử mảng thông báo chưa đọc");
printjson(users.aggregate([
  { $match: { MaUser: "USR-DEMO-BUYER" } },
  { $project: { _id: 0, MaUser: 1, ChuaDoc: { $filter: {
      input: "$ThongBao", as: "tb", cond: { $eq: ["$$tb.DaDoc", false] }
  } } } }
]).toArray());

print("8. Kế hoạch truy vấn và executionStats thực tế trên máy này");
printjson(users.find({ VaiTro: "moi_gioi" }).explain("executionStats"));
