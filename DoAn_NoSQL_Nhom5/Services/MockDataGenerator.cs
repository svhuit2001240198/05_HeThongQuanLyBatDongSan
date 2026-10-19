using System.Text;
using System.Text.Json;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace DoAn_NoSQL_Nhom5.Services;

public sealed class MockDataGenerator(IWebHostEnvironment env, IPasswordHasher<User> hasher)
{
    public const string DemoPassword = "Demo@12345";
    private readonly Random _random = new(20261010);
    private static readonly DateTime BaseDate = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly string[] Areas = ["TP. Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Bình Dương", "Đồng Nai"];
    private static readonly string[] Family = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Võ", "Đặng", "Bùi"];
    private static readonly string[] Given = ["Minh Anh", "Hoàng Nam", "Quốc Huy", "Ngọc Linh", "Thanh Trúc", "Gia Bảo", "Khánh Vy", "Đức Phúc", "Tuấn Kiệt", "Hải Yến"];

    public async Task GenerateAsync(int count)
    {
        var folder = Path.Combine(env.ContentRootPath, "Data", "mock"); Directory.CreateDirectory(folder);
        var users = new List<User>
        {
            Demo("ADMIN", "Quản trị HomeLand", "admin", UserRoles.Admin),
            Demo("BUYER", "Nguyễn Công Danh", "buyer", UserRoles.Buyer),
            Demo("SELLER", "Phạm Quang Vinh", "seller", UserRoles.Seller),
            Demo("BROKER", "Trần Minh Anh", "broker", UserRoles.Broker),
            Demo("BROKER2", "Lê Hoàng Nam", "broker2", UserRoles.Broker)
        };
        // Các tài khoản giả định dùng chung mật khẩu demo. Tài khoản đăng ký thật luôn được băm riêng.
        var mockHash = hasher.HashPassword(new User(), DemoPassword);
        for (var i = 5; i < count; i++)
        {
            var role = i % 10 < 6 ? UserRoles.Buyer : i % 10 < 8 ? UserRoles.Seller : UserRoles.Broker;
            var name = Family[_random.Next(Family.Length)] + " " + Given[_random.Next(Given.Length)];
            users.Add(new User { MaUser = "USR-" + i.ToString("D5"), HoTen = name,
                Email = $"user{i:D5}@example.test", SDT = "09" + (10000000 + i), MatKhau = mockHash,
                VaiTro = role, NgayTao = BaseDate.AddDays(-_random.Next(1, 730)), SecurityStamp = Guid.NewGuid().ToString("N"),
                GioiTinh = i % 2 == 0 ? "nam" : "nu",
                ThongTinMoiGioi = role == UserRoles.Broker ? new ThongTinMoiGioiDetail { KinhNghiem = _random.Next(1, 16),
                    GioiThieu = "Hỗ trợ khách hàng tìm bất động sản phù hợp và tư vấn thông tin trước khi giao dịch.",
                    KhuVucHoatDong = Areas.OrderBy(_ => _random.Next()).Take(_random.Next(1, 4)).ToList(),
                    TrangThai = i % 17 == 0 ? "ngung_hoat_dong" : "dang_hoat_dong" } : null });
        }
        var buyers = users.Where(x => x.VaiTro == UserRoles.Buyer && !x.MaUser.Contains("DEMO")).ToList();
        foreach (var user in users)
        {
            var words = user.HoTen.Split(' '); user.Ho = words[0]; user.Ten = words[^1]; user.TenDem = string.Join(' ', words.Skip(1).Take(Math.Max(0, words.Length - 2)));
            user.BDSDaLuu = Enumerable.Range(1, 18).OrderBy(_ => _random.Next()).Take(_random.Next(0, 7)).Select(i => $"BDS-DEMO-{i:D3}").ToList();
            for (var j = 0; j < _random.Next(0, 4); j++) user.ThongBao.Add(new ThongBaoUser { MaThongBao = $"TB-{user.MaUser}-{j}", LoaiThongBao = "he_thong",
                TieuDe = j == 0 ? "Hoàn thiện thông tin tài khoản" : "Khám phá những tin đăng mới",
                NoiDung = j == 0 ? "Cập nhật hồ sơ để khách hàng và môi giới dễ dàng kết nối với bạn." : "Bạn có thể xem và lưu những bất động sản phù hợp với nhu cầu của mình.",
                DaDoc = j % 2 == 1, NgayTao = BaseDate.AddDays(-j), MaBDS = j == 0 ? null : $"BDS-DEMO-{j:D3}" });
            if (user.VaiTro == UserRoles.Broker)
                foreach (var reviewer in Enumerable.Range(0, 8).Select(_ => buyers[_random.Next(buyers.Count)]).DistinctBy(x => x.MaUser).Take(_random.Next(0, 6)))
                    user.DanhGia.Add(new DanhGiaMoiGioi { MaUserDanhGia = reviewer.MaUser, SoSao = _random.Next(3, 6), NoiDung = "Tư vấn rõ ràng, hỗ trợ nhiệt tình và phản hồi đúng hẹn.", NgayDanhGia = BaseDate.AddDays(-_random.Next(1, 60)) });
        }
        users[1].BDSDaLuu = ["BDS-DEMO-001", "BDS-DEMO-003", "BDS-DEMO-005", "BDS-DEMO-007"];
        await WriteAsync(Path.Combine(folder, "Users.jsonl"), users);
        await WriteAsync(Path.Combine(folder, "BDS-demo.jsonl"), Properties());
        await using var csv = new StreamWriter(Path.Combine(folder, "MaUser_VaiTro.csv"), false, new UTF8Encoding(true));
        await csv.WriteLineAsync("MaUser,VaiTro,HoTen");
        foreach (var u in users) await csv.WriteLineAsync($"{u.MaUser},{u.VaiTro},\"{u.HoTen.Replace("\"", "\"\"")}\"");
        await File.WriteAllTextAsync(Path.Combine(folder, "ThongKeDuLieu.json"), JsonSerializer.Serialize(new
        {
            Users = users.Count, BDS_demo = 18, VaiTro = users.GroupBy(x => x.VaiTro).ToDictionary(g => g.Key, g => g.Count()),
            UsersCoHoSoMoiGioi = users.Count(x => x.ThongTinMoiGioi is not null),
            TongDanhGia = users.Sum(x => x.DanhGia.Count), TongThongBao = users.Sum(x => x.ThongBao.Count),
            TongMaBDSDaLuu = users.Sum(x => x.BDSDaLuu.Count)
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Đã sinh {count:N0} Users và 18 BĐS demo vào {folder}.");
    }

    private User Demo(string code, string name, string email, string role)
    {
        var user = new User { MaUser = "USR-DEMO-" + code, HoTen = name, Email = email + "@homeland.test", SDT = "0901234567", VaiTro = role,
            NgayTao = BaseDate.AddDays(-30), SecurityStamp = Guid.NewGuid().ToString("N"),
            ThongTinMoiGioi = role == UserRoles.Broker ? new ThongTinMoiGioiDetail { KinhNghiem = 8, GioiThieu = "Tư vấn căn hộ và nhà ở tại TP. Hồ Chí Minh. Đồng hành cùng khách hàng từ tìm kiếm đến xem nhà.", KhuVucHoatDong = ["TP. Hồ Chí Minh", "Bình Dương"] } : null };
        user.MatKhau = hasher.HashPassword(user, DemoPassword); return user;
    }

    private static IEnumerable<BatDongSan> Properties()
    {
        string[] titles = ["Căn hộ ánh sáng tại Thảo Điền", "Biệt thự sân vườn ven sông", "Đất nền khu dân cư An Phú", "Căn hộ ban công nhìn thành phố", "Biệt thự hiện đại tại Bình Dương", "Đất nền gần công viên", "Căn hộ hai phòng ngủ tại Phú Mỹ Hưng", "Biệt thự không gian mở", "Đất nền mặt tiền Đồng Nai"];
        for (var i = 1; i <= 18; i++)
        {
            var type = (i - 1) % 3; var city = i % 4 == 0 ? "Bình Dương" : i % 5 == 0 ? "Đồng Nai" : "TP. Hồ Chí Minh";
            yield return new BatDongSan { MaBDS = $"BDS-DEMO-{i:D3}", MaChuSoHuu = "USR-DEMO-SELLER", MaMoiGioi = i % 2 == 1 ? "USR-DEMO-BROKER" : "USR-DEMO-BROKER2",
                LoaiBDS = new[] { "can_ho", "biet_thu", "dat_nen" }[type], TieuDe = titles[(i - 1) % titles.Length],
                DiaChi = new DiaChiBDS { SoNha = (12 + i).ToString(), Duong = "Đường số " + i, Phuong = "An Phú", Quan = "Thủ Đức", ThanhPho = city },
                ViTri = new ViTriGeographic { KinhDo = 106.72 + i * 0.001, ViDo = 10.79 + i * 0.001 }, DienTich = type == 0 ? 65 + i * 2 : 150 + i * 5,
                Gia = type == 0 ? 3_200_000_000m + i * 100_000_000m : type == 1 ? 12_000_000_000m + i * 200_000_000m : 2_400_000_000m + i * 100_000_000m,
                Huong = "Đông Nam", MoTa = "Không gian thoáng, thuận tiện di chuyển và gần các tiện ích sinh hoạt. Liên hệ chủ sở hữu hoặc môi giới để tìm hiểu thông tin và sắp xếp xem thực tế.",
                HinhAnh = [new HinhAnhBDS { URL = "/img/property-" + new[] { "apartment", "villa", "land" }[type] + ".svg", MoTa = "Hình minh họa bất động sản mẫu", ThuTu = 1 }],
                TienIch = ["Bãi đỗ xe", "Công viên", "An ninh 24/7"],
                ThuocTinhRieng = type == 0 ? new BsonDocument { { "SoPhongNgu", 2 }, { "SoPhongTam", 2 }, { "Tang", i + 3 } }
                    : type == 1 ? new BsonDocument { { "SoPhongNgu", 4 }, { "SoPhongTam", 3 }, { "SoTang", 2 }, { "CoHoBoi", i % 2 == 0 }, { "CoSanVuon", true }, { "ChoDoOto", true } }
                    : new BsonDocument { { "ChieuDai", 20 }, { "ChieuRong", 8 }, { "LoaiDat", "Đất ở" }, { "PhapLy", "Sổ hồng" } },
                ThongTinGiaoDich = new ThongTinGiaoDichBDS { LoaiGiaoDich = "ban", TrangThai = "dang_ban", NgayDang = BaseDate.AddDays(-i), NgayHetHan = BaseDate.AddDays(90) },
                LichSuGia = [] };
        }
    }

    private static async Task WriteAsync<T>(string path, IEnumerable<T> items)
    {
        await using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
        foreach (var item in items)
        {
            var bson = item.ToBsonDocument(); bson.Remove("_id");
            await writer.WriteLineAsync(bson.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson }));
        }
    }
}
