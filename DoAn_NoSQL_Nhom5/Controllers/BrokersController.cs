using System.Security.Claims;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Services;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_NoSQL_Nhom5.Controllers;

public sealed class BrokersController(UserService users, PropertyService properties) : Controller
{
    public async Task<IActionResult> Index(string area = "", int minExperience = 0, int page = 1) => View(new BrokerListVM
    { Result = await users.BrokersAsync(area, minExperience, page), Area = area, MinExperience = Math.Clamp(minExperience, 0, 60) });
    public async Task<IActionResult> Details(string id, int page = 1)
    {
        var broker = await users.GetAsync(id);
        if (broker?.VaiTro != UserRoles.Broker || broker.TrangThaiTaiKhoan == "bi_khoa") return NotFound();
        var author = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(new BrokerDetailVM { Broker = broker, Properties = await properties.SearchAsync("", "", "", page, id), ReviewerNames = await users.NamesAsync(broker.DanhGia.Select(x => x.MaUserDanhGia)), Review = new ReviewVM { BrokerCode = id }, HasReviewed = broker.DanhGia.Any(x => x.MaUserDanhGia == author) });
    }
    [Authorize, HttpPost]
    public async Task<IActionResult> Review(ReviewVM model)
    {
        if (!ModelState.IsValid) TempData["Error"] = "Chọn 1–5 sao và viết đánh giá từ 5 đến 1.000 ký tự.";
        else
        {
            try { var added = await users.AddReviewAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, model); TempData[added ? "Success" : "Error"] = added ? "Cảm ơn bạn. Đánh giá đã được gửi." : "Bạn đã đánh giá môi giới này trước đó."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        }
        return RedirectToAction(nameof(Details), new { id = model.BrokerCode });
    }
}
