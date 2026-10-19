using System.Security.Claims;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Services;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_NoSQL_Nhom5.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public sealed class AdminController(UserService users) : Controller
{
    public async Task<IActionResult> Index(string search = "", string role = "", int page = 1) => View(new AdminListVM
    { Result = await users.SearchAsync(search, role, page), RoleCounts = await users.RoleCountsAsync(), Search = search, Role = role });
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var u = await users.GetAsync(id);
        return u is null ? NotFound() : View(new AdminEditVM { MaUser = u.MaUser, VaiTro = u.VaiTro, IsLocked = u.TrangThaiTaiKhoan == "bi_khoa", HoTen = u.HoTen, Email = u.Email });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(AdminEditVM model)
    {
        var target = await users.GetAsync(model.MaUser); if (target is null) return NotFound();
        model.HoTen = target.HoTen; model.Email = target.Email;
        if (!ModelState.IsValid) return View(model);
        try { await users.AdminUpdateAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, model); TempData["Success"] = "Đã cập nhật quyền và trạng thái tài khoản."; return RedirectToAction(nameof(Index)); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); return View(model); }
    }
}
