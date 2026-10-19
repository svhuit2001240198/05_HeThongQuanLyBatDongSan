using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoAn_NoSQL_Nhom5.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("MaUser")]
        public string MaUser { get; set; } = string.Empty;
        [BsonElement("Ho")]
        public string Ho { get; set; } = string.Empty;

        [BsonElement("TenDem")]
        public string TenDem { get; set; } = string.Empty;

        [BsonElement("Ten")]
        public string Ten { get; set; } = string.Empty;

        [BsonElement("HoTen")]
        public string HoTen { get; set; } = string.Empty;
        [BsonElement("GioiTinh")]
        public string GioiTinh { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("SDT")]
        public string SDT { get; set; } = string.Empty;

        [BsonElement("MatKhau")]
        public string MatKhau { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò: "nguoi_mua", "nguoi_ban", "moi_gioi", "admin"
        /// </summary>
        [BsonElement("VaiTro")]
        public string VaiTro { get; set; } = string.Empty;

        [BsonElement("TrangThaiTaiKhoan")]
        public string TrangThaiTaiKhoan { get; set; } = "hoat_dong";

        // Đổi giá trị này khi đổi mật khẩu hoặc quyền để thu hồi cookie cũ.
        [BsonElement("SecurityStamp")]
        public string SecurityStamp { get; set; } = string.Empty;

        [BsonElement("NgayTao")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [BsonElement("ThongTinMoiGioi")]
        [BsonIgnoreIfNull]
        public ThongTinMoiGioiDetail? ThongTinMoiGioi { get; set; }

        [BsonElement("DanhGia")]
        public List<DanhGiaMoiGioi> DanhGia { get; set; } = new();

        [BsonElement("BDSDaLuu")]
        public List<string> BDSDaLuu { get; set; } = new();

        [BsonElement("ThongBao")]
        public List<ThongBaoUser> ThongBao { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class ThongTinMoiGioiDetail
    {
        [BsonElement("AnhDaiDien")]
        public string AnhDaiDien { get; set; } = string.Empty;

        [BsonElement("GioiThieu")]
        public string GioiThieu { get; set; } = string.Empty;

        [BsonElement("KinhNghiem")]
        public int KinhNghiem { get; set; }

        [BsonElement("KhuVucHoatDong")]
        public List<string> KhuVucHoatDong { get; set; } = new();

        /// <summary>
        /// Trạng thái: "dang_hoat_dong", "ngung_hoat_dong"
        /// </summary>
        [BsonElement("TrangThai")]
        public string TrangThai { get; set; } = "dang_hoat_dong";
    }

    [BsonIgnoreExtraElements]
    public class DanhGiaMoiGioi
    {
        [BsonElement("MaUserDanhGia")]
        public string MaUserDanhGia { get; set; } = string.Empty;

        [BsonElement("MaBDS")]
        [BsonIgnoreIfNull]
        public string? MaBDS { get; set; }

        [BsonElement("SoSao")]
        public int SoSao { get; set; }

        [BsonElement("NoiDung")]
        public string NoiDung { get; set; } = string.Empty;

        [BsonElement("NgayDanhGia")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;
    }

    [BsonIgnoreExtraElements]
    public class ThongBaoUser
    {
        [BsonElement("MaThongBao")]
        public string MaThongBao { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("LoaiThongBao")]
        public string LoaiThongBao { get; set; } = string.Empty;

        [BsonElement("TieuDe")]
        public string TieuDe { get; set; } = string.Empty;

        [BsonElement("NoiDung")]
        public string NoiDung { get; set; } = string.Empty;

        [BsonElement("MaBDS")]
        [BsonIgnoreIfNull]
        public string? MaBDS { get; set; }

        [BsonElement("MaLichHen")]
        [BsonIgnoreIfNull]
        public string? MaLichHen { get; set; }

        [BsonElement("DaDoc")]
        public bool DaDoc { get; set; } = false;

        [BsonElement("NgayTao")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}
