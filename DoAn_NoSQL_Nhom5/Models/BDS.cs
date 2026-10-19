using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoAn_NoSQL_Nhom5.Models
{
    [BsonIgnoreExtraElements]
    public class BatDongSan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("MaBDS")]
        public string MaBDS { get; set; } = string.Empty;

        [BsonElement("MaChuSoHuu")]
        public string MaChuSoHuu { get; set; } = string.Empty;

        [BsonElement("MaMoiGioi")]
        [BsonIgnoreIfNull]
        public string? MaMoiGioi { get; set; }

        /// <summary>
        /// Loại BDS: "can_ho", "dat_nen", "biet_thu"
        /// </summary>
        [BsonElement("LoaiBDS")]
        public string LoaiBDS { get; set; } = string.Empty;

        [BsonElement("TieuDe")]
        public string TieuDe { get; set; } = string.Empty;

        [BsonElement("DiaChi")]
        public DiaChiBDS DiaChi { get; set; } = new();

        [BsonElement("ViTri")]
        public ViTriGeographic ViTri { get; set; } = new();

        [BsonElement("DienTich")]
        public double DienTich { get; set; }

        [BsonElement("Gia")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Gia { get; set; }

        [BsonElement("Huong")]
        public string Huong { get; set; } = string.Empty;

        [BsonElement("MoTa")]
        public string MoTa { get; set; } = string.Empty;

        [BsonElement("HinhAnh")]
        public List<HinhAnhBDS> HinhAnh { get; set; } = new();

        [BsonElement("Video")]
        public List<VideoBDS> Video { get; set; } = new();

        [BsonElement("TienIch")]
        public List<string> TienIch { get; set; } = new();

        /// <summary>
        /// Thuộc tính động lưu dưới dạng BsonDocument hoặc object Polymorphic tùy loại BDS
        /// </summary>
        [BsonElement("ThuocTinhRieng")]
        public BsonDocument ThuocTinhRieng { get; set; } = new();

        [BsonElement("ThongTinGiaoDich")]
        public ThongTinGiaoDichBDS ThongTinGiaoDich { get; set; } = new();

        [BsonElement("LichSuGia")]
        public List<LichSuGiaBDS> LichSuGia { get; set; } = new();
    }

    public class DiaChiBDS
    {
        [BsonElement("SoNha")]
        public string SoNha { get; set; } = string.Empty;

        [BsonElement("Duong")]
        public string Duong { get; set; } = string.Empty;

        [BsonElement("Phuong")]
        public string Phuong { get; set; } = string.Empty;

        [BsonElement("Quan")]
        public string Quan { get; set; } = string.Empty;

        [BsonElement("ThanhPho")]
        public string ThanhPho { get; set; } = string.Empty;
    }

    public class ViTriGeographic
    {
        [BsonElement("KinhDo")]
        public double KinhDo { get; set; }

        [BsonElement("ViDo")]
        public double ViDo { get; set; }
    }

    public class HinhAnhBDS
    {
        [BsonElement("URL")]
        public string URL { get; set; } = string.Empty;

        [BsonElement("MoTa")]
        public string MoTa { get; set; } = string.Empty;

        [BsonElement("ThuTu")]
        public int ThuTu { get; set; }
    }

    public class VideoBDS
    {
        [BsonElement("URL")]
        public string URL { get; set; } = string.Empty;

        [BsonElement("MoTa")]
        public string MoTa { get; set; } = string.Empty;
    }

    public class ThongTinGiaoDichBDS
    {
        /// <summary>
        /// Loại giao dịch: "ban", "cho_thue"
        /// </summary>
        [BsonElement("LoaiGiaoDich")]
        public string LoaiGiaoDich { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái: "dang_ban", "da_dat_coc", "da_ban", "dang_cho_thue", "da_cho_thue", "tam_an", "het_han"
        /// </summary>
        [BsonElement("TrangThai")]
        public string TrangThai { get; set; } = "dang_ban";

        [BsonElement("NgayDang")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime NgayDang { get; set; } = DateTime.UtcNow;

        [BsonElement("NgayHetHan")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime NgayHetHan { get; set; }
    }

    public class LichSuGiaBDS
    {
        [BsonElement("GiaCu")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal GiaCu { get; set; }

        [BsonElement("GiaMoi")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal GiaMoi { get; set; }

        [BsonElement("ThoiGian")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ThoiGian { get; set; } = DateTime.UtcNow;

        [BsonElement("MaNguoiThayDoi")]
        public string MaNguoiThayDoi { get; set; } = string.Empty;
    }

    #region Strongly-Typed Class cho ThuocTinhRieng
    public class ThuocTinhCanHo
    {
        [BsonElement("SoPhongNgu")]
        public int SoPhongNgu { get; set; }

        [BsonElement("SoPhongTam")]
        public int SoPhongTam { get; set; }

        [BsonElement("Tang")]
        public int Tang { get; set; }
    }

    public class ThuocTinhDatNen
    {
        [BsonElement("ChieuDai")]
        public double ChieuDai { get; set; }

        [BsonElement("ChieuRong")]
        public double ChieuRong { get; set; }

        [BsonElement("LoaiDat")]
        public string LoaiDat { get; set; } = string.Empty;

        [BsonElement("PhapLy")]
        public string PhapLy { get; set; } = string.Empty;
    }

    public class ThuocTinhBietThu
    {
        [BsonElement("SoPhongNgu")]
        public int SoPhongNgu { get; set; }

        [BsonElement("SoPhongTam")]
        public int SoPhongTam { get; set; }

        [BsonElement("SoTang")]
        public int SoTang { get; set; }

        [BsonElement("CoHoBoi")]
        public bool CoHoBoi { get; set; }

        [BsonElement("CoSanVuon")]
        public bool CoSanVuon { get; set; }

        [BsonElement("ChoDoOto")]
        public bool ChoDoOto { get; set; }
    }
    #endregion
}
