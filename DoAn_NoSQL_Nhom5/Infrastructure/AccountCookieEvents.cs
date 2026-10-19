using System.Security.Claims;
using DoAn_NoSQL_Nhom5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DoAn_NoSQL_Nhom5.Models;

namespace DoAn_NoSQL_Nhom5.Infrastructure;

public sealed class AccountCookieEvents(UserService users) : CookieAuthenticationEvents
{
    public static ClaimsPrincipal Principal(User user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.MaUser), new Claim(ClaimTypes.Name, user.HoTen),
        new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.VaiTro),
        new Claim("SecurityStamp", user.SecurityStamp)
    }, CookieAuthenticationDefaults.AuthenticationScheme));

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var code = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = code is null ? null : await users.GetAsync(code);
        if (user is null || user.TrangThaiTaiKhoan == "bi_khoa" || user.SecurityStamp != context.Principal?.FindFirstValue("SecurityStamp") || user.VaiTro != context.Principal?.FindFirstValue(ClaimTypes.Role))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
