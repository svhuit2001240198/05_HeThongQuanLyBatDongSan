using System.Text.RegularExpressions;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Services;

public sealed class UserService(MongoContext db, IPasswordHasher<User> hasher)
{
    private static readonly SemaphoreSlim AdminChanges = new(1, 1);
    private static string EmailKey(string value) => value.Trim().ToLowerInvariant();
    public Task<User?> GetAsync(string code) => db.Users.Find(x => x.MaUser == code).FirstOrDefaultAsync()!;
    public Task<User?> GetByEmailAsync(string email) => db.Users.Find(x => x.Email == EmailKey(email)).FirstOrDefaultAsync()!;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        if (user is null || user.TrangThaiTaiKhoan == "bi_khoa") return null;
        PasswordVerificationResult check;
        try { check = hasher.VerifyHashedPassword(user, user.MatKhau, password); }
        catch (FormatException)
        {
            // Stored password may be plaintext or non-standard; compare it below and upgrade on success.
            check = PasswordVerificationResult.Failed;
        }
        catch (ArgumentException) { return null; }
        // Some plaintext values (including "12345678") are valid Base64, so the hasher can return
        // Failed without throwing FormatException. Check legacy plaintext in either case, then upgrade it.
        if (check == PasswordVerificationResult.Failed)
        {
            if (!string.Equals(user.MatKhau, password, StringComparison.Ordinal)) return null;
            user.MatKhau = hasher.HashPassword(user, password);
            check = PasswordVerificationResult.Success;
        }
        var update = Builders<User>.Update.Set(x => x.SecurityStamp, string.IsNullOrEmpty(user.SecurityStamp) ? Guid.NewGuid().ToString("N") : user.SecurityStamp);
        if (check == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MatKhau = hasher.HashPassword(user, password);
            update = update.Set(x => x.MatKhau, user.MatKhau);
        }
        var result = await db.Users.FindOneAndUpdateAsync(x => x.MaUser == user.MaUser, update, new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<User> RegisterAsync(RegisterVM model)
    {
        await db.EnsureUserIndexesAsync();
        if (!UserRoles.PublicRoles.Contains(model.VaiTro)) throw new InvalidOperationException("Vai trò đăng ký không hợp lệ.");
        var user = new User
        {
            MaUser = "USR-" + Guid.NewGuid().ToString("N"), HoTen = model.HoTen.Trim(),
            Email = EmailKey(model.Email), SDT = model.SDT.Trim(), VaiTro = model.VaiTro,
            SecurityStamp = Guid.NewGuid().ToString("N"), NgayTao = DateTime.UtcNow,
            ThongTinMoiGioi = model.VaiTro == UserRoles.Broker ? new ThongTinMoiGioiDetail() : null
        };
        SetNameParts(user);
        user.MatKhau = hasher.HashPassword(user, model.Password);
        user.ThongBao.Add(new ThongBaoUser { LoaiThongBao = "he_thong", TieuDe = "Chào mừng đến HomeLand", NoiDung = "Hoàn thiện hồ sơ để bắt đầu kết nối và lưu những bất động sản bạn quan tâm." });
        await db.Users.InsertOneAsync(user);
        return user;
    }

    public async Task UpdateProfileAsync(string code, ProfileVM model)
    {
        await db.EnsureUserIndexesAsync();
        var user = await GetAsync(code) ?? throw new InvalidOperationException("Tài khoản không còn tồn tại.");
        user.HoTen = model.HoTen.Trim(); SetNameParts(user);
        var update = Builders<User>.Update.Set(x => x.HoTen, user.HoTen)
            .Set(x => x.Ho, user.Ho).Set(x => x.TenDem, user.TenDem).Set(x => x.Ten, user.Ten)
            .Set(x => x.Email, EmailKey(model.Email)).Set(x => x.SDT, model.SDT.Trim())
            .Set(x => x.GioiTinh, model.GioiTinh).Set(x => x.SecurityStamp, Guid.NewGuid().ToString("N"));
        if (user.VaiTro == UserRoles.Broker)
        {
            var areas = model.KhuVucHoatDong.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().Take(15).ToList();
            update = update.Set(x => x.ThongTinMoiGioi, new ThongTinMoiGioiDetail
            {
                AnhDaiDien = user.ThongTinMoiGioi?.AnhDaiDien ?? "", GioiThieu = model.GioiThieu.Trim(),
                KinhNghiem = model.KinhNghiem, KhuVucHoatDong = areas,
                TrangThai = model.TrangThaiMoiGioi == "ngung_hoat_dong" ? "ngung_hoat_dong" : "dang_hoat_dong"
            });
        }
        await db.Users.UpdateOneAsync(x => x.MaUser == code, update);
    }

    public async Task<bool> ChangePasswordAsync(string code, PasswordVM model)
    {
        var user = await GetAsync(code);
        if (user is null) return false;
        var currentPasswordValid = false;
        try { currentPasswordValid = hasher.VerifyHashedPassword(user, user.MatKhau, model.CurrentPassword) != PasswordVerificationResult.Failed; }
        catch (FormatException)
        {
            // Legacy plaintext passwords are checked below and replaced with a hash on successful change.
        }
        catch (ArgumentException) { return false; }
        if (!currentPasswordValid && !string.Equals(user.MatKhau, model.CurrentPassword, StringComparison.Ordinal)) return false;
        await db.Users.UpdateOneAsync(x => x.MaUser == code,
            Builders<User>.Update.Set(x => x.MatKhau, hasher.HashPassword(user, model.NewPassword))
                .Set(x => x.SecurityStamp, Guid.NewGuid().ToString("N")));
        return true;
    }

    public async Task<bool> SavePropertyAsync(string code, string bdsCode)
    {
        var property = await db.Properties.Find(x => x.MaBDS == bdsCode).FirstOrDefaultAsync();
        if (property is null || property.ThongTinGiaoDich.TrangThai is "tam_an" or "het_han") return false;
        var user = await GetAsync(code);
        if (user is null) return false;
        if (user.BDSDaLuu.Count >= 200 && !user.BDSDaLuu.Contains(bdsCode)) throw new InvalidOperationException("Bạn đã lưu 200 tin. Hãy bỏ bớt tin trước khi lưu thêm.");
        await db.Users.UpdateOneAsync(x => x.MaUser == code, Builders<User>.Update.AddToSet(x => x.BDSDaLuu, bdsCode));
        return true;
    }

    public Task RemoveSavedAsync(string code, string bdsCode) => db.Users.UpdateOneAsync(x => x.MaUser == code, Builders<User>.Update.Pull(x => x.BDSDaLuu, bdsCode));
    public Task ClearSavedAsync(string code) => db.Users.UpdateOneAsync(x => x.MaUser == code, Builders<User>.Update.Set(x => x.BDSDaLuu, new List<string>()));

    public async Task<List<ThongBaoUser>> GetNotificationsAsync(string code)
        => (await GetAsync(code))?.ThongBao.OrderByDescending(x => x.NgayTao).ToList() ?? [];

    public Task ReadNotificationAsync(string code, string notificationCode)
    {
        var f = Builders<User>.Filter;
        return db.Users.UpdateOneAsync(f.Eq(x => x.MaUser, code) & f.ElemMatch(x => x.ThongBao, x => x.MaThongBao == notificationCode),
            Builders<User>.Update.Set("ThongBao.$.DaDoc", true));
    }

    public Task ReadAllNotificationsAsync(string code) => db.Users.UpdateOneAsync(x => x.MaUser == code, Builders<User>.Update.Set("ThongBao.$[].DaDoc", true));
    public Task DeleteNotificationAsync(string code, string notificationCode) => db.Users.UpdateOneAsync(x => x.MaUser == code, Builders<User>.Update.PullFilter(x => x.ThongBao, x => x.MaThongBao == notificationCode));
    public Task AddNotificationAsync(string code, ThongBaoUser notification) => db.Users.UpdateOneAsync(x => x.MaUser == code,
        Builders<User>.Update.PushEach(x => x.ThongBao, new[] { notification }, slice: -100));

    public async Task<PagedResult<User>> SearchAsync(string search, string role, int page, int pageSize = 15)
    {
        var f = Builders<User>.Filter; var filter = f.Empty;
        search = (search ?? "").Trim();
        if (search.Length > 0)
        {
            var regex = new BsonRegularExpression(Regex.Escape(search.Length > 100 ? search[..100] : search), "i");
            filter &= f.Regex(x => x.HoTen, regex) | f.Regex(x => x.Email, regex) | f.Eq(x => x.MaUser, search);
        }
        if (UserRoles.All.Contains(role)) filter &= f.Eq(x => x.VaiTro, role);
        var total = await db.Users.CountDocumentsAsync(filter);
        page = ClampPage(page, total, pageSize);
        var items = await db.Users.Find(filter).SortByDescending(x => x.NgayTao).ThenBy(x => x.MaUser)
            .Skip((page - 1) * pageSize).Limit(pageSize)
            .Project<User>(Builders<User>.Projection.Exclude(x => x.MatKhau).Exclude(x => x.SecurityStamp)).ToListAsync();
        return new PagedResult<User> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Dictionary<string, long>> RoleCountsAsync()
    {
        var rows = await db.Users.Aggregate().Group<BsonDocument>(new BsonDocument { { "_id", "$VaiTro" }, { "SoLuong", new BsonDocument("$sum", 1) } }).ToListAsync();
        return rows.Where(x => x["_id"].IsString).ToDictionary(x => x["_id"].AsString, x => x["SoLuong"].ToInt64());
    }

    public async Task<PagedResult<User>> BrokersAsync(string area, int minExperience, int page)
    {
        var f = Builders<User>.Filter;
        var filter = f.Eq(x => x.VaiTro, UserRoles.Broker) & f.Ne(x => x.TrangThaiTaiKhoan, "bi_khoa") & f.Eq("ThongTinMoiGioi.TrangThai", "dang_hoat_dong");
        if (!string.IsNullOrWhiteSpace(area)) filter &= f.Regex("ThongTinMoiGioi.KhuVucHoatDong", new BsonRegularExpression(Regex.Escape(area.Trim()), "i"));
        if (minExperience > 0) filter &= f.Gte("ThongTinMoiGioi.KinhNghiem", Math.Clamp(minExperience, 0, 60));
        var count = await db.Users.CountDocumentsAsync(filter); page = ClampPage(page, count, 12);
        var users = await db.Users.Find(filter).SortByDescending(x => x.ThongTinMoiGioi!.KinhNghiem).ThenBy(x => x.MaUser)
            .Skip((page - 1) * 12).Limit(12).Project<User>(Builders<User>.Projection.Exclude(x => x.MatKhau).Exclude(x => x.SecurityStamp)).ToListAsync();
        return new PagedResult<User> { Items = users, Total = count, Page = page };
    }

    public async Task<bool> AddReviewAsync(string author, ReviewVM model)
    {
        if (author == model.BrokerCode) throw new InvalidOperationException("Bạn không thể tự đánh giá hồ sơ của mình.");
        var broker = await GetAsync(model.BrokerCode);
        if (broker?.VaiTro != UserRoles.Broker || broker.TrangThaiTaiKhoan == "bi_khoa") throw new InvalidOperationException("Hồ sơ môi giới không khả dụng.");
        if (!string.IsNullOrWhiteSpace(model.MaBDS))
        {
            var bds = await db.Properties.Find(x => x.MaBDS == model.MaBDS && x.MaMoiGioi == model.BrokerCode).FirstOrDefaultAsync();
            if (bds is null) throw new InvalidOperationException("Bất động sản không thuộc hồ sơ môi giới này.");
        }
        var f = Builders<User>.Filter;
        var result = await db.Users.UpdateOneAsync(f.Eq(x => x.MaUser, model.BrokerCode)
            & f.Not(f.ElemMatch(x => x.DanhGia, x => x.MaUserDanhGia == author)),
            Builders<User>.Update.Push(x => x.DanhGia, new DanhGiaMoiGioi
            {
                MaUserDanhGia = author, MaBDS = string.IsNullOrWhiteSpace(model.MaBDS) ? null : model.MaBDS,
                SoSao = model.SoSao, NoiDung = model.NoiDung.Trim(), NgayDanhGia = DateTime.UtcNow
            }));
        if (result.ModifiedCount == 1) await AddNotificationAsync(model.BrokerCode, new ThongBaoUser
        {
            LoaiThongBao = "danh_gia", TieuDe = "Bạn có một đánh giá mới", NoiDung = $"Một khách hàng vừa đánh giá dịch vụ của bạn {model.SoSao}/5 sao.", MaBDS = model.MaBDS
        });
        return result.ModifiedCount == 1;
    }

    public async Task<Dictionary<string, string>> NamesAsync(IEnumerable<string> codes)
    {
        var list = codes.Distinct().ToList();
        if (list.Count == 0) return [];
        var users = await db.Users.Find(Builders<User>.Filter.In(x => x.MaUser, list)).Project(x => new { x.MaUser, x.HoTen }).ToListAsync();
        return users.ToDictionary(x => x.MaUser, x => x.HoTen);
    }

    public async Task AdminUpdateAsync(string actor, AdminEditVM model)
    {
        if (!UserRoles.All.Contains(model.VaiTro)) throw new InvalidOperationException("Vai trò không hợp lệ.");
        if (actor == model.MaUser && (model.IsLocked || model.VaiTro != UserRoles.Admin)) throw new InvalidOperationException("Bạn không thể tự khóa hoặc tự bỏ quyền quản trị.");
        await AdminChanges.WaitAsync();
        try
        {
            var user = await GetAsync(model.MaUser) ?? throw new InvalidOperationException("Người dùng không tồn tại.");
            if (user.VaiTro == UserRoles.Admin && (model.IsLocked || model.VaiTro != UserRoles.Admin))
            {
                var count = await db.Users.CountDocumentsAsync(x => x.VaiTro == UserRoles.Admin && x.TrangThaiTaiKhoan != "bi_khoa");
                if (count <= 1) throw new InvalidOperationException("Cần giữ ít nhất một quản trị viên đang hoạt động.");
            }
            var update = Builders<User>.Update.Set(x => x.VaiTro, model.VaiTro)
                .Set(x => x.TrangThaiTaiKhoan, model.IsLocked ? "bi_khoa" : "hoat_dong")
                .Set(x => x.SecurityStamp, Guid.NewGuid().ToString("N"));
            update = model.VaiTro == UserRoles.Broker
                ? update.Set(x => x.ThongTinMoiGioi, user.ThongTinMoiGioi ?? new ThongTinMoiGioiDetail())
                : update.Unset(x => x.ThongTinMoiGioi);
            await db.Users.UpdateOneAsync(x => x.MaUser == model.MaUser, update);
        }
        finally { AdminChanges.Release(); }
    }

    public static int ClampPage(int page, long count, int size) => Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)size)));
    private static void SetNameParts(User u)
    {
        var parts = u.HoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        u.Ho = parts.Length > 1 ? parts[0] : ""; u.Ten = parts.LastOrDefault() ?? "";
        u.TenDem = parts.Length > 2 ? string.Join(' ', parts.Skip(1).Take(parts.Length - 2)) : "";
    }
}
