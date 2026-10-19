using System.Globalization;
using DoAn_NoSQL_Nhom5.Models;

namespace DoAn_NoSQL_Nhom5.Infrastructure;

public static class Ui
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    public static string Role(string? value) => value switch
    {
        UserRoles.Buyer => "Người mua", UserRoles.Seller => "Người bán",
        UserRoles.Broker => "Môi giới", UserRoles.Admin => "Quản trị viên", _ => "Người dùng"
    };
    public static string Initials(string? name)
    {
        var words = (name ?? "U").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length == 0 ? "U" : string.Concat(words.TakeLast(2).Select(w => char.ToUpperInvariant(w[0])));
    }
    public static string Date(DateTime date) => date.ToUniversalTime().AddHours(7).ToString("dd/MM/yyyy", Vi);
    public static string DateTimeText(DateTime date) => date.ToUniversalTime().AddHours(7).ToString("HH:mm · dd/MM/yyyy", Vi);
    public static string Number(long n) => n.ToString("N0", Vi);
    public static string Price(decimal p) => p >= 1_000_000_000 ? (p / 1_000_000_000).ToString("0.##", Vi) + " tỷ" : p >= 1_000_000 ? (p / 1_000_000).ToString("0.##", Vi) + " triệu" : p.ToString("N0", Vi) + " đ";
    public static string PropertyType(string? type) => type switch { "can_ho" => "Căn hộ", "dat_nen" => "Đất nền", "biet_thu" => "Biệt thự", _ => "Bất động sản" };
    public static string Status(string? status) => status switch
    {
        "dang_ban" => "Đang bán", "da_dat_coc" => "Đã đặt cọc", "da_ban" => "Đã bán",
        "dang_cho_thue" => "Đang cho thuê", "da_cho_thue" => "Đã cho thuê",
        "tam_an" => "Tạm ẩn", "het_han" => "Hết hạn", _ => "Đang cập nhật"
    };
    public static string PropertyImage(BatDongSan p)
    {
        var url = p.HinhAnh.OrderBy(x => x.ThuTu).FirstOrDefault()?.URL;
        if (!string.IsNullOrEmpty(url) && ((url.StartsWith('/') && !url.StartsWith("//")) || (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "https"))) return url;
        return "/img/property-apartment.svg";
    }
    public static double Rating(User u) => u.DanhGia.Count == 0 ? 0 : u.DanhGia.Average(x => x.SoSao);
}
