using System.Diagnostics;
using System.Security.Claims;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using DoAn_NoSQL_Nhom5.Services;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_NoSQL_Nhom5.Controllers;

public sealed class HomeController(PropertyService properties, UserService users, RedisHistoryService redis) : Controller
{
    public async Task<IActionResult> Index()
    {
        var codes = await redis.HotCodesAsync(); var hot = await properties.ByCodesAsync(codes);
        var featured = codes.Select(c => hot.FirstOrDefault(x => x.MaBDS == c)).OfType<BatDongSan>().Take(6).ToList();
        if (featured.Count < 6)
        {
            var latest = (await properties.SearchAsync("", "", "", 1)).Items;
            featured = featured.Concat(latest).DistinctBy(x => x.MaBDS).Take(6).ToList();
        }
        return View(new HomeVM { Featured = featured });
    }
    public async Task<IActionResult> Properties(string search = "", string type = "", string city = "", int page = 1) => View(new PropertyListVM
    { Result = await properties.SearchAsync(search, type, city, page), Search = search, Type = type, City = city });
    public async Task<IActionResult> Details(string id)
    {
        var property = await properties.GetAsync(id); if (property is null) return NotFound();
        var code = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = code is null ? null : await users.GetAsync(code);
        await redis.TrackAsync(code, property.MaBDS);
        var history = property.LichSuGia.Select(item => new PropertyHistoryItem
        {
            Title = "Giá bất động sản được cập nhật",
            Description = $"Giá thay đổi từ {Ui.Price(item.GiaCu)} thành {Ui.Price(item.GiaMoi)}.",
            Date = item.ThoiGian,
            Type = "price"
        }).ToList();
        history.Add(new PropertyHistoryItem
        {
            Title = "Tin đăng được tạo",
            Description = "Bất động sản được đăng tải lên hệ thống.",
            Date = property.ThongTinGiaoDich.NgayDang,
            Type = "created"
        });
        return View(new PropertyDetailVM { Property = property, Owner = await users.GetAsync(property.MaChuSoHuu),
            Broker = string.IsNullOrEmpty(property.MaMoiGioi) ? null : await users.GetAsync(property.MaMoiGioi),
            IsSaved = account?.BDSDaLuu.Contains(property.MaBDS) == true,
            CanManage = User.IsInRole(UserRoles.Admin) || property.MaChuSoHuu == code || property.MaMoiGioi == code,
            History = history });
    }
    public IActionResult Contact() => View();
    public IActionResult Login() => RedirectToAction("Login", "Account");
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() { Response.StatusCode = 500; return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    public IActionResult Status(int code) { Response.StatusCode = code; ViewBag.StatusCode = code; return View(); }
}
