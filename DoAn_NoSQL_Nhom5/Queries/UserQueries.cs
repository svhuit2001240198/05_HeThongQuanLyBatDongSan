using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Queries;

// Các truy vấn riêng để trình bày trong chương 2. Không có thao tác xóa dữ liệu.
public sealed class UserQueries(MongoContext db)
{
    public Task<List<User>> ByRoleAsync(string role) => db.Users.Find(x => x.VaiTro == role)
        .Project<User>(Builders<User>.Projection.Exclude(x => x.MatKhau).Exclude(x => x.SecurityStamp)).Limit(20).ToListAsync();

    public Task<List<User>> BrokersInAreaAsync(string area, int experience) => db.Users.Find(
        Builders<User>.Filter.Eq(x => x.VaiTro, UserRoles.Broker)
        & Builders<User>.Filter.AnyEq("ThongTinMoiGioi.KhuVucHoatDong", area)
        & Builders<User>.Filter.Gte("ThongTinMoiGioi.KinhNghiem", experience))
        .Project<User>(Builders<User>.Projection.Exclude(x => x.MatKhau).Exclude(x => x.SecurityStamp)).Limit(20).ToListAsync();

    public Task<List<BsonDocument>> SavedWithLookupAsync(string code) => db.Users.Aggregate()
        .Match(x => x.MaUser == code)
        .Project<BsonDocument>(new BsonDocument { { "_id", 0 }, { "MaUser", 1 }, { "BDSDaLuu", 1 } })
        .AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
        {
            { "from", db.Properties.CollectionNamespace.CollectionName },
            { "localField", "BDSDaLuu" }, { "foreignField", "MaBDS" }, { "as", "TinDaLuu" }
        })).ToListAsync();

    public Task<List<BsonDocument>> TopBrokersAsync() => db.Users.Aggregate()
        .Match(x => x.VaiTro == UserRoles.Broker && x.TrangThaiTaiKhoan != "bi_khoa")
        .Unwind<BsonDocument>("DanhGia")
        .Group<BsonDocument>(new BsonDocument
        {
            { "_id", "$MaUser" }, { "HoTen", new BsonDocument("$first", "$HoTen") },
            { "DiemTrungBinh", new BsonDocument("$avg", "$DanhGia.SoSao") },
            { "SoDanhGia", new BsonDocument("$sum", 1) }
        })
        .Sort(new BsonDocument { { "DiemTrungBinh", -1 }, { "SoDanhGia", -1 }, { "_id", 1 } }).Limit(10).ToListAsync();

    public Task<List<BsonDocument>> RegistrationsByMonthAsync() => db.Users.Aggregate()
        .Group<BsonDocument>(new BsonDocument
        {
            { "_id", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m" }, { "date", "$NgayTao" }, { "timezone", "+07:00" } }) },
            { "SoLuong", new BsonDocument("$sum", 1) }
        }).Sort(new BsonDocument("_id", 1)).ToListAsync();

    public Task<BsonDocument> ExplainRoleQueryAsync() => db.Database.RunCommandAsync<BsonDocument>(new BsonDocument
    {
        { "explain", new BsonDocument { { "find", db.Users.CollectionNamespace.CollectionName }, { "filter", new BsonDocument("VaiTro", UserRoles.Broker) } } },
        { "verbosity", "executionStats" }
    });
}
