using System.Security.Claims;
using DoAn_NoSQL_Nhom5.Models;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Services;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Controllers;

[Authorize]
public sealed class AccountController(UserService users, PropertyService properties, RedisHistoryService redis) : Controller
{
    private string Code => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(Index)) : View(new LoginVM { ReturnUrl = returnUrl });
    [AllowAnonymous, HttpPost, EnableRateLimiting("account-login")]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.AuthenticateAsync(model.Email, model.Password);
        if (user is null) { ModelState.AddModelError("", "Email hoặc mật khẩu chưa đúng, hoặc tài khoản đã bị khóa."); return View(model); }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AccountCookieEvents.Principal(user), new AuthenticationProperties
        { IsPersistent = model.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 14 : 1) });
        return !string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl) : RedirectToAction(nameof(Index));
    }
    [AllowAnonymous, HttpGet]
    public IActionResult Register() => User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(Index)) : View(new RegisterVM());
    [AllowAnonymous, HttpPost, EnableRateLimiting("account-login")]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!UserRoles.PublicRoles.Contains(model.VaiTro)) ModelState.AddModelError(nameof(model.VaiTro), "Chọn một loại tài khoản hợp lệ.");
        if (!ModelState.IsValid) return View(model);
        try
        {
            var user = await users.RegisterAsync(model);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AccountCookieEvents.Principal(user));
            TempData["Success"] = "Tài khoản đã được tạo. Chào mừng bạn đến HomeLand!";
            return RedirectToAction(nameof(Index));
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        { ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký."); return View(model); }
    }
    [HttpPost]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction("Index", "Home"); }
    public async Task<IActionResult> Index()
    {
        var account = await users.GetAsync(Code); if (account is null) return Challenge();
        return View(new DashboardVM { Account = account, Saved = (await properties.ByCodesAsync(account.BDSDaLuu)).Take(3).ToList(), UnreadCount = account.ThongBao.Count(x => !x.DaDoc), ManagedCount = await properties.ManagedCountAsync(Code, IsAdmin) });
    }
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var a = await users.GetAsync(Code); if (a is null) return Challenge();
        return View(new ProfileVM { HoTen = a.HoTen, Email = a.Email, SDT = a.SDT, GioiTinh = a.GioiTinh, VaiTro = a.VaiTro,
            GioiThieu = a.ThongTinMoiGioi?.GioiThieu ?? "", KinhNghiem = a.ThongTinMoiGioi?.KinhNghiem ?? 0,
            KhuVucHoatDong = string.Join(", ", a.ThongTinMoiGioi?.KhuVucHoatDong ?? []), TrangThaiMoiGioi = a.ThongTinMoiGioi?.TrangThai ?? "dang_hoat_dong" });
    }
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileVM model)
    {
        var a = await users.GetAsync(Code); if (a is null) return Challenge(); model.VaiTro = a.VaiTro;
        model.GioiTinh ??= ""; model.GioiThieu ??= ""; model.KhuVucHoatDong ??= "";
        model.TrangThaiMoiGioi ??= "dang_hoat_dong";
        if (!new[] { "", "nam", "nu", "khac" }.Contains(model.GioiTinh)) ModelState.AddModelError(nameof(model.GioiTinh), "Lựa chọn không hợp lệ.");
        if (!ModelState.IsValid) return View(model);
        try
        {
            await users.UpdateProfileAsync(Code, model); var updated = await users.GetAsync(Code);
            var auth = await HttpContext.AuthenticateAsync();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AccountCookieEvents.Principal(updated!), auth.Properties);
            TempData["Success"] = "Đã cập nhật hồ sơ của bạn."; return RedirectToAction(nameof(Profile));
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        { ModelState.AddModelError(nameof(model.Email), "Email này đang được một tài khoản khác sử dụng."); return View(model); }
    }
    [HttpGet] public IActionResult ChangePassword() => View(new PasswordVM());
    [HttpPost]
    public async Task<IActionResult> ChangePassword(PasswordVM model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.CurrentPassword == model.NewPassword) { ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới cần khác mật khẩu hiện tại."); return View(model); }
        if (!await users.ChangePasswordAsync(Code, model)) { ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại chưa đúng."); return View(model); }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Đã đổi mật khẩu. Vui lòng đăng nhập lại."; return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public async Task<IActionResult> ManagedProperties(int page = 1)
    {
        if (!CanManageProperties) return Forbid();
        return View(new ManagedPropertiesVM { Result = await properties.ManagedListAsync(Code, IsAdmin, page) });
    }
    [HttpGet]
    public IActionResult CreateProperty()
    {
        if (!CanManageProperties) return Forbid();
        return View("PropertyForm", new PropertyFormVM());
    }
    [HttpPost]
    public async Task<IActionResult> CreateProperty(PropertyFormVM model)
    {
        if (!CanManageProperties) return Forbid();
        ValidatePropertyForm(model);
        if (!ModelState.IsValid) return View("PropertyForm", model);

        var property = ToProperty(model, null);
        property.MaChuSoHuu = Code;
        property.MaMoiGioi = User.IsInRole(UserRoles.Broker) ? Code : null;
        await properties.CreateAsync(property);
        TempData["Success"] = "Tin bất động sản đã được đăng.";
        return RedirectToAction(nameof(ManagedProperties));
    }
    [HttpGet]
    public async Task<IActionResult> EditProperty(string id)
    {
        if (!CanManageProperties) return Forbid();
        var property = await properties.GetManagedAsync(id, Code, IsAdmin);
        return property is null ? NotFound() : View("PropertyForm", ToForm(property));
    }
    [HttpPost]
    public async Task<IActionResult> EditProperty(string id, PropertyFormVM model)
    {
        if (!CanManageProperties) return Forbid();
        if (!string.Equals(id, model.MaBDS, StringComparison.Ordinal))
            return BadRequest();
        var current = await properties.GetManagedAsync(id, Code, IsAdmin);
        if (current is null) return NotFound();
        ValidatePropertyForm(model);
        if (!ModelState.IsValid) return View("PropertyForm", model);

        var updated = ToProperty(model, current);
        if (!await properties.UpdateAsync(id, updated, Code, IsAdmin)) return NotFound();
        TempData["Success"] = "Thông tin bất động sản đã được cập nhật.";
        return RedirectToAction(nameof(ManagedProperties));
    }
    [HttpPost]
    public async Task<IActionResult> DeleteProperty(string id)
    {
        if (!CanManageProperties) return Forbid();
        if (!await properties.DeleteAsync(id, Code, IsAdmin)) return NotFound();
        TempData["Success"] = "Tin bất động sản đã được xóa.";
        return RedirectToAction(nameof(ManagedProperties));
    }
    public async Task<IActionResult> Favorites(int page = 1)
    { var user = await users.GetAsync(Code); return user is null ? Challenge() : View(await properties.FavoritesAsync(user, page)); }
    [HttpPost]
    public async Task<IActionResult> Save(string maBDS, string? returnUrl = null)
    {
        try { var saved = await users.SavePropertyAsync(Code, maBDS); TempData[saved ? "Success" : "Error"] = saved ? "Đã lưu bất động sản vào danh sách yêu thích." : "Tin không còn khả dụng."; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return Back(returnUrl);
    }
    [HttpPost] public async Task<IActionResult> Unsave(string maBDS, string? returnUrl = null) { await users.RemoveSavedAsync(Code, maBDS); TempData["Success"] = "Đã bỏ lưu tin."; return Back(returnUrl); }
    [HttpPost] public async Task<IActionResult> ClearFavorites() { await users.ClearSavedAsync(Code); TempData["Success"] = "Đã xóa danh sách tin đã lưu."; return RedirectToAction(nameof(Favorites)); }
    public async Task<IActionResult> Notifications(bool unread = false)
    { ViewBag.Unread = unread; var list = await users.GetNotificationsAsync(Code); return View(unread ? list.Where(x => !x.DaDoc).ToList() : list); }
    [HttpPost] public async Task<IActionResult> ReadNotification(string maThongBao) { await users.ReadNotificationAsync(Code, maThongBao); return RedirectToAction(nameof(Notifications)); }
    [HttpPost] public async Task<IActionResult> ReadAll() { await users.ReadAllNotificationsAsync(Code); TempData["Success"] = "Đã đánh dấu tất cả thông báo là đã đọc."; return RedirectToAction(nameof(Notifications)); }
    [HttpPost] public async Task<IActionResult> DeleteNotification(string maThongBao) { await users.DeleteNotificationAsync(Code, maThongBao); return RedirectToAction(nameof(Notifications)); }
    public async Task<IActionResult> History()
    {
        var history = await redis.HistoryAsync(Code); var list = await properties.ByCodesAsync(history.Items.Select(x => x.MaBDS)); var byCode = list.ToDictionary(x => x.MaBDS);
        return View(new HistoryVM { Available = history.Available, Items = history.Items.Where(x => byCode.ContainsKey(x.MaBDS)).Select(x => (byCode[x.MaBDS], x.ViewedAt)).ToList() });
    }
    [HttpPost] public async Task<IActionResult> ClearHistory() { await redis.ClearAsync(Code); return RedirectToAction(nameof(History)); }
    [AllowAnonymous] public IActionResult AccessDenied() => View();
    private IActionResult Back(string? url) => url is not null && Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToAction(nameof(Favorites));
    private bool IsAdmin => User.IsInRole(UserRoles.Admin);
    private bool CanManageProperties => IsAdmin || User.IsInRole(UserRoles.Seller) || User.IsInRole(UserRoles.Broker);

    private void ValidatePropertyForm(PropertyFormVM model)
    {
        if (!new[] { "can_ho", "dat_nen", "biet_thu" }.Contains(model.LoaiBDS))
            ModelState.AddModelError(nameof(model.LoaiBDS), "Loại bất động sản không hợp lệ.");
        if (!new[] { "ban", "cho_thue" }.Contains(model.LoaiGiaoDich))
            ModelState.AddModelError(nameof(model.LoaiGiaoDich), "Loại giao dịch không hợp lệ.");
        if (!new[] { "dang_ban", "da_dat_coc", "da_ban", "dang_cho_thue", "da_cho_thue", "tam_an", "het_han" }.Contains(model.TrangThai))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái tin đăng không hợp lệ.");
        if (model.NgayHetHan.Date < DateTime.UtcNow.Date)
            ModelState.AddModelError(nameof(model.NgayHetHan), "Ngày hết hạn không được ở quá khứ.");
        var imageUrls = (model.HinhAnhUrls ?? "").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (imageUrls.Length > 10)
            ModelState.AddModelError(nameof(model.HinhAnhUrls), "Chỉ được đăng tối đa 10 ảnh.");
        if (imageUrls.Any(url =>
            {
                var isLocal = url.StartsWith('/') && !url.StartsWith("//") && !url.Contains('\\');
                var isHttps = Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
                return !isLocal && !isHttps;
            }))
            ModelState.AddModelError(nameof(model.HinhAnhUrls), "Ảnh chỉ chấp nhận đường dẫn nội bộ hoặc URL HTTPS.");
    }

    private static PropertyFormVM ToForm(BatDongSan property) => new()
    {
        MaBDS = property.MaBDS,
        TieuDe = property.TieuDe,
        LoaiBDS = property.LoaiBDS,
        LoaiGiaoDich = property.ThongTinGiaoDich.LoaiGiaoDich,
        TrangThai = property.ThongTinGiaoDich.TrangThai,
        DienTich = property.DienTich,
        Gia = property.Gia,
        Huong = property.Huong,
        MoTa = property.MoTa,
        SoNha = property.DiaChi.SoNha,
        Duong = property.DiaChi.Duong,
        Phuong = property.DiaChi.Phuong,
        Quan = property.DiaChi.Quan,
        ThanhPho = property.DiaChi.ThanhPho,
        HinhAnhUrls = string.Join(Environment.NewLine, property.HinhAnh.OrderBy(x => x.ThuTu).Select(x => x.URL)),
        TienIch = string.Join(", ", property.TienIch),
        NgayHetHan = property.ThongTinGiaoDich.NgayHetHan == default
            ? DateTime.UtcNow.Date.AddDays(90)
            : property.ThongTinGiaoDich.NgayHetHan.ToLocalTime().Date
    };

    private static BatDongSan ToProperty(PropertyFormVM form, BatDongSan? current)
    {
        var imageUrls = (form.HinhAnhUrls ?? "").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new BatDongSan
        {
            MaBDS = current?.MaBDS ?? "",
            MaChuSoHuu = current?.MaChuSoHuu ?? "",
            MaMoiGioi = current?.MaMoiGioi,
            LoaiBDS = form.LoaiBDS,
            TieuDe = form.TieuDe.Trim(),
            DiaChi = new DiaChiBDS
            {
                SoNha = (form.SoNha ?? "").Trim(),
                Duong = (form.Duong ?? "").Trim(),
                Phuong = (form.Phuong ?? "").Trim(),
                Quan = (form.Quan ?? "").Trim(),
                ThanhPho = (form.ThanhPho ?? "").Trim()
            },
            ViTri = current?.ViTri ?? new ViTriGeographic(),
            DienTich = form.DienTich,
            Gia = form.Gia,
            Huong = (form.Huong ?? "").Trim(),
            MoTa = form.MoTa.Trim(),
            HinhAnh = imageUrls.Select((url, index) => new HinhAnhBDS { URL = url, ThuTu = index }).ToList(),
            Video = current?.Video ?? [],
            TienIch = (form.TienIch ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList(),
            ThuocTinhRieng = current?.ThuocTinhRieng ?? new(),
            ThongTinGiaoDich = new ThongTinGiaoDichBDS
            {
                LoaiGiaoDich = form.LoaiGiaoDich,
                TrangThai = form.TrangThai,
                NgayDang = current?.ThongTinGiaoDich.NgayDang ?? DateTime.UtcNow,
                NgayHetHan = DateTime.SpecifyKind(form.NgayHetHan.Date, DateTimeKind.Utc)
            },
            LichSuGia = current?.LichSuGia ?? []
        };
    }
}
