using System.Text.RegularExpressions;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using DoAn_NoSQL_Nhom5.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Services;

public sealed class PropertyService(MongoContext db)
{
    private FilterDefinition<BatDongSan> Visible => Builders<BatDongSan>.Filter.Nin("ThongTinGiaoDich.TrangThai", new[] { "tam_an", "het_han" });

    public async Task<BatDongSan?> GetAsync(string code) =>
        await db.Properties.Find(Builders<BatDongSan>.Filter.Eq(x => x.MaBDS, code) & Visible).FirstOrDefaultAsync();

    public Task<List<BatDongSan>> ByCodesAsync(IEnumerable<string> codes) =>
        db.Properties.Find(Builders<BatDongSan>.Filter.In(x => x.MaBDS, codes) & Visible).ToListAsync();

    public Task<long> ManagedCountAsync(string code, bool isAdmin = false) =>
        db.Properties.CountDocumentsAsync(isAdmin
            ? Builders<BatDongSan>.Filter.Empty
            : Builders<BatDongSan>.Filter.Or(
                Builders<BatDongSan>.Filter.Eq(x => x.MaChuSoHuu, code),
                Builders<BatDongSan>.Filter.Eq(x => x.MaMoiGioi, code)));

    public async Task<PagedResult<BatDongSan>> SearchAsync(string search, string type, string city, int page, string? brokerCode = null)
    {
        var f = Builders<BatDongSan>.Filter;
        var filter = Visible;
        if (!string.IsNullOrWhiteSpace(search)) filter &= f.Regex(x => x.TieuDe, new BsonRegularExpression(Regex.Escape(search.Trim()), "i"));
        if (new[] { "can_ho", "dat_nen", "biet_thu" }.Contains(type)) filter &= f.Eq(x => x.LoaiBDS, type);
        if (!string.IsNullOrWhiteSpace(city)) filter &= f.Regex("DiaChi.ThanhPho", new BsonRegularExpression(Regex.Escape(city.Trim()), "i"));
        if (brokerCode is not null) filter &= f.Eq(x => x.MaMoiGioi, brokerCode);
        var count = await db.Properties.CountDocumentsAsync(filter);
        page = UserService.ClampPage(page, count, 12);
        var items = await db.Properties.Find(filter).SortByDescending(x => x.ThongTinGiaoDich.NgayDang)
            .ThenBy(x => x.MaBDS).Skip((page - 1) * 12).Limit(12).ToListAsync();
        return new PagedResult<BatDongSan> { Items = items, Total = count, Page = page };
    }

    public async Task<FavoriteVM> FavoritesAsync(User user, int page)
    {
        var f = Builders<BatDongSan>.Filter;
        var filter = f.In(x => x.MaBDS, user.BDSDaLuu) & Visible;
        var count = await db.Properties.CountDocumentsAsync(filter);
        page = UserService.ClampPage(page, count, 12);
        var rows = await db.Properties.Find(filter).SortByDescending(x => x.ThongTinGiaoDich.NgayDang)
            .ThenBy(x => x.MaBDS).Skip((page - 1) * 12).Limit(12).ToListAsync();
        return new FavoriteVM { Result = new PagedResult<BatDongSan> { Items = rows, Total = count, Page = page }, MissingCount = user.BDSDaLuu.Count - (int)count };
    }

    public async Task<BatDongSan> CreateAsync(BatDongSan model)
    {
        model.MaBDS = "BDS-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        model.ThongTinGiaoDich.NgayDang = DateTime.UtcNow;
        await db.Properties.InsertOneAsync(model);
        return model;
    }

    public async Task<BatDongSan?> GetManagedAsync(string code, string userCode, bool isAdmin = false)
    {
        var f = Builders<BatDongSan>.Filter;
        var authorization = isAdmin ? f.Empty : f.Or(f.Eq(x => x.MaChuSoHuu, userCode), f.Eq(x => x.MaMoiGioi, userCode));
        return await db.Properties.Find(f.Eq(x => x.MaBDS, code) & authorization).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(string maBds, BatDongSan update, string userCode, bool isAdmin = false)
    {
        var f = Builders<BatDongSan>.Filter;
        var authorization = isAdmin ? f.Empty : f.Or(f.Eq(x => x.MaChuSoHuu, userCode), f.Eq(x => x.MaMoiGioi, userCode));
        var current = await db.Properties.Find(f.Eq(x => x.MaBDS, maBds) & authorization).FirstOrDefaultAsync();
        if (current is null) return false;

        var u = Builders<BatDongSan>.Update
            .Set(x => x.TieuDe, update.TieuDe)
            .Set(x => x.LoaiBDS, update.LoaiBDS)
            .Set(x => x.DiaChi, update.DiaChi)
            .Set(x => x.ViTri, update.ViTri)
            .Set(x => x.DienTich, update.DienTich)
            .Set(x => x.Gia, update.Gia)
            .Set(x => x.Huong, update.Huong)
            .Set(x => x.MoTa, update.MoTa)
            .Set(x => x.HinhAnh, update.HinhAnh)
            .Set(x => x.Video, update.Video)
            .Set(x => x.TienIch, update.TienIch)
            .Set(x => x.ThuocTinhRieng, update.ThuocTinhRieng)
            .Set(x => x.ThongTinGiaoDich.LoaiGiaoDich, update.ThongTinGiaoDich.LoaiGiaoDich)
            .Set(x => x.ThongTinGiaoDich.TrangThai, update.ThongTinGiaoDich.TrangThai)
            .Set(x => x.ThongTinGiaoDich.NgayHetHan, update.ThongTinGiaoDich.NgayHetHan);
        if (current.Gia != update.Gia)
        {
            u = u.Push(x => x.LichSuGia, new LichSuGiaBDS
            {
                GiaCu = current.Gia,
                GiaMoi = update.Gia,
                ThoiGian = DateTime.UtcNow,
                MaNguoiThayDoi = userCode
            });
        }
        var result = await db.Properties.UpdateOneAsync(f.Eq(x => x.MaBDS, maBds) & authorization, u);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string maBds, string userCode, bool isAdmin = false)
    {
        var f = Builders<BatDongSan>.Filter;
        var authorization = isAdmin ? f.Empty : f.Or(f.Eq(x => x.MaChuSoHuu, userCode), f.Eq(x => x.MaMoiGioi, userCode));
        var result = await db.Properties.DeleteOneAsync(f.Eq(x => x.MaBDS, maBds) & authorization);
        return result.DeletedCount > 0;
    }

    public async Task<PagedResult<BatDongSan>> ManagedListAsync(string userCode, bool isAdmin = false, int page = 1)
    {
        var f = Builders<BatDongSan>.Filter;
        var filter = isAdmin ? f.Empty : f.Or(f.Eq(x => x.MaChuSoHuu, userCode), f.Eq(x => x.MaMoiGioi, userCode));
        var count = await db.Properties.CountDocumentsAsync(filter);
        page = UserService.ClampPage(page, count, 12);
        var items = await db.Properties.Find(filter).SortByDescending(x => x.ThongTinGiaoDich.NgayDang)
            .ThenBy(x => x.MaBDS).Skip((page - 1) * 12).Limit(12).ToListAsync();
        return new PagedResult<BatDongSan> { Items = items, Total = count, Page = page };
    }
}
