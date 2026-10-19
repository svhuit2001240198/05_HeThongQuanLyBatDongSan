using System.Threading.RateLimiting;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using DoAn_NoSQL_Nhom5.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

var maintenance = args.Contains("--generate-data") || args.Contains("--seed") || args.Contains("--indexes");
var builder = WebApplication.CreateBuilder(maintenance ? Array.Empty<string>() : args);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddMvcOptions(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddSingleton<RedisHistoryService>();
builder.Services.AddScoped<AccountCookieEvents>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<MockDataGenerator>();
builder.Services.AddScoped<DoAn_NoSQL_Nhom5.Queries.UserQueries>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied";
    o.Cookie.Name = "HomeLand.Account"; o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax; o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    o.ExpireTimeSpan = TimeSpan.FromDays(1); o.SlidingExpiration = true;
    o.EventsType = typeof(AccountCookieEvents);
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("account-login", c => RateLimitPartition.GetFixedWindowLimiter(
        c.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
    o.OnRejected = async (c, ct) =>
    {
        c.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await c.HttpContext.Response.WriteAsync("Bạn đã thử quá nhiều lần. Vui lòng thử lại sau một phút.", ct);
    };
});
var app = builder.Build();
if (args.Contains("--generate-data") || args.Contains("--seed") || args.Contains("--indexes"))
{
    using var scope = app.Services.CreateScope();
    try
    {
        if (args.Contains("--generate-data"))
        {
            var i = Array.IndexOf(args, "--generate-data");
            var count = i + 1 < args.Length && int.TryParse(args[i + 1], out var n) ? n : 10000;
            await scope.ServiceProvider.GetRequiredService<MockDataGenerator>().GenerateAsync(Math.Clamp(count, 10000, 50000));
        }
        else
        {
            await scope.ServiceProvider.GetRequiredService<MongoContext>().CreateIndexesAsync();
            if (args.Contains("--seed")) await scope.ServiceProvider.GetRequiredService<SeedService>().RunAsync();
        }
    }
    catch (Exception ex) { Console.Error.WriteLine("Không hoàn thành: " + ex.Message); Environment.ExitCode = 1; }
    return;
}
try { await app.Services.GetRequiredService<MongoContext>().CreateIndexesAsync(); }
catch (Exception ex) { app.Logger.LogError("Chưa khởi tạo được index MongoDB: {Reason}. Kiểm tra appsettings và chạy --seed.", ex.Message); }
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
