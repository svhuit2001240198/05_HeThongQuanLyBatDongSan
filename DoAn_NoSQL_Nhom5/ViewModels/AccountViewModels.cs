using System.ComponentModel.DataAnnotations;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;

namespace DoAn_NoSQL_Nhom5.ViewModels;

public sealed class LoginVM
{
    [Required(ErrorMessage = "Nhập email của bạn."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(160)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Nhập mật khẩu."), StringLength(128)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class RegisterVM
{
    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(160)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Nhập số điện thoại."), RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SĐT gồm 10–11 số, bắt đầu bằng 0.")]
    public string SDT { get; set; } = "";
    [Required(ErrorMessage = "Nhập mật khẩu."), StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu từ 8 đến 128 ký tự.")]
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "Nhập lại mật khẩu."), Compare(nameof(Password), ErrorMessage = "Hai mật khẩu chưa khớp.")]
    public string ConfirmPassword { get; set; } = "";
    public string VaiTro { get; set; } = UserRoles.Buyer;
}

public sealed class ProfileVM
{
    [Required(ErrorMessage = "Nhập họ và tên."), StringLength(100, MinimumLength = 2)]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Nhập email của bạn."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(160)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Nhập số điện thoại."), RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SĐT gồm 10–11 số, bắt đầu bằng 0.")]
    public string SDT { get; set; } = "";
    [StringLength(20)] public string GioiTinh { get; set; } = "";
    [StringLength(1200)] public string GioiThieu { get; set; } = "";
    [Range(0, 60, ErrorMessage = "Kinh nghiệm từ 0 đến 60 năm.")] public int KinhNghiem { get; set; }
    [StringLength(500)] public string KhuVucHoatDong { get; set; } = "";
    public string TrangThaiMoiGioi { get; set; } = "dang_hoat_dong";
    // Chỉ dùng để hiển thị. Controller luôn lấy vai trò thật từ tài khoản đã đăng nhập.
    public string VaiTro { get; set; } = "";
}

public sealed class PasswordVM
{
    [Required(ErrorMessage = "Nhập mật khẩu hiện tại.")] public string CurrentPassword { get; set; } = "";
    [Required(ErrorMessage = "Nhập mật khẩu mới."), StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu mới từ 8 đến 128 ký tự.")]
    public string NewPassword { get; set; } = "";
    [Required(ErrorMessage = "Nhập lại mật khẩu mới."), Compare(nameof(NewPassword), ErrorMessage = "Hai mật khẩu chưa khớp.")]
    public string ConfirmPassword { get; set; } = "";
}

public sealed class ReviewVM
{
    [Required] public string BrokerCode { get; set; } = "";
    [Range(1, 5, ErrorMessage = "Chọn từ 1 đến 5 sao.")] public int SoSao { get; set; } = 5;
    [Required(ErrorMessage = "Nhập nội dung đánh giá."), StringLength(1000, MinimumLength = 5)] public string NoiDung { get; set; } = "";
    [StringLength(100)] public string? MaBDS { get; set; }
}

public sealed class AdminEditVM
{
    [Required] public string MaUser { get; set; } = "";
    [Required] public string VaiTro { get; set; } = "";
    public bool IsLocked { get; set; }
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public long Total { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public sealed class DashboardVM
{
    public User Account { get; set; } = new();
    public List<BatDongSan> Saved { get; set; } = [];
    public int UnreadCount { get; set; }
    public long ManagedCount { get; set; }
}

public sealed class FavoriteVM
{
    public PagedResult<BatDongSan> Result { get; set; } = new();
    public int MissingCount { get; set; }
}

public sealed class ManagedPropertiesVM
{
    public PagedResult<BatDongSan> Result { get; set; } = new();
}

public sealed class BrokerListVM
{
    public PagedResult<User> Result { get; set; } = new();
    public string Area { get; set; } = "";
    public int MinExperience { get; set; }
}

public sealed class BrokerDetailVM
{
    public User Broker { get; set; } = new();
    public PagedResult<BatDongSan> Properties { get; set; } = new();
    public Dictionary<string, string> ReviewerNames { get; set; } = [];
    public ReviewVM Review { get; set; } = new();
    public bool HasReviewed { get; set; }
}

public sealed class PropertyListVM
{
    public PagedResult<BatDongSan> Result { get; set; } = new();
    public string Search { get; set; } = "";
    public string Type { get; set; } = "";
    public string City { get; set; } = "";
}

public sealed class PropertyDetailVM
{
    public BatDongSan Property { get; set; } = new();
    public User? Owner { get; set; }
    public User? Broker { get; set; }
    public bool IsSaved { get; set; }
    public bool CanManage { get; set; }
    public List<PropertyHistoryItem> History { get; set; } = [];
}

public sealed class PropertyFormVM
{
    public string MaBDS { get; set; } = "";
    [Required(ErrorMessage = "Nhập tiêu đề tin đăng."), StringLength(140, MinimumLength = 5)]
    public string TieuDe { get; set; } = "";
    [Required] public string LoaiBDS { get; set; } = "can_ho";
    [Required] public string LoaiGiaoDich { get; set; } = "ban";
    [Required] public string TrangThai { get; set; } = "dang_ban";
    [Range(1, 1_000_000_000, ErrorMessage = "Diện tích phải lớn hơn 0.")]
    public double DienTich { get; set; }
    [Range(typeof(decimal), "1", "1000000000000000", ErrorMessage = "Giá phải lớn hơn 0.")]
    public decimal Gia { get; set; }
    [StringLength(60)] public string Huong { get; set; } = "";
    [Required(ErrorMessage = "Nhập mô tả bất động sản."), StringLength(5000, MinimumLength = 20)]
    public string MoTa { get; set; } = "";
    [StringLength(100)] public string SoNha { get; set; } = "";
    [Required(ErrorMessage = "Nhập tên đường."), StringLength(160)] public string Duong { get; set; } = "";
    [StringLength(100)] public string Phuong { get; set; } = "";
    [StringLength(100)] public string Quan { get; set; } = "";
    [Required(ErrorMessage = "Nhập tỉnh/thành phố."), StringLength(100)] public string ThanhPho { get; set; } = "";
    [StringLength(2000)] public string HinhAnhUrls { get; set; } = "";
    [StringLength(1000)] public string TienIch { get; set; } = "";
    [DataType(DataType.Date)] public DateTime NgayHetHan { get; set; } = DateTime.UtcNow.Date.AddDays(90);
}

public sealed class HomeVM
{
    public List<BatDongSan> Featured { get; set; } = [];
}

public sealed class AdminListVM
{
    public PagedResult<User> Result { get; set; } = new();
    public Dictionary<string, long> RoleCounts { get; set; } = [];
    public string Search { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class ViewHistoryItem
{
    public string MaBDS { get; set; } = "";
    public DateTime ViewedAt { get; set; }
}

public sealed class HistoryVM
{
    public bool Available { get; set; }
    public List<(BatDongSan Property, DateTime ViewedAt)> Items { get; set; } = [];
}

public sealed class PaginationVM
{
    public int Page { get; set; }
    public int Pages { get; set; }
    public long Total { get; set; }
}
public sealed class PropertyHistoryItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string Type { get; set; } = "updated"; // created, price, updated, status, checked
}
