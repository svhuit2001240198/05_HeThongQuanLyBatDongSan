using DoAn_NoSQL_Nhom5.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Infrastructure;

public sealed class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<BatDongSan> Properties { get; }

    public MongoContext(IOptions<MongoDBSettings> options)
    {
        var config = options.Value;
        var settings = MongoClientSettings.FromConnectionString(config.ConnectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        var client = new MongoClient(settings);
        Database = client.GetDatabase(config.DatabaseName);
        Users = Database.GetCollection<User>(config.UsersCollection);
        Properties = Database.GetCollection<BatDongSan>(config.BDSCollection);
    }

    public async Task CreateIndexesAsync()
    {
        await EnsureUserIndexesAsync();
        var b = Builders<BatDongSan>.IndexKeys;
        await Properties.Indexes.CreateManyAsync([
            new CreateIndexModel<BatDongSan>(b.Ascending(x => x.MaBDS), new CreateIndexOptions { Unique = true, Name = "uq_bds_code" }),
            new CreateIndexModel<BatDongSan>(b.Ascending(x => x.MaChuSoHuu), new CreateIndexOptions { Name = "bds_owner" }),
            new CreateIndexModel<BatDongSan>(b.Ascending(x => x.MaMoiGioi), new CreateIndexOptions { Name = "bds_broker" })
        ]);
    }

    public async Task EnsureUserIndexesAsync()
    {
        var key = Builders<User>.IndexKeys;
        await Users.Indexes.CreateManyAsync([
            new CreateIndexModel<User>(key.Ascending(x => x.MaUser), new CreateIndexOptions { Unique = true, Name = "uq_user_code" }),
            new CreateIndexModel<User>(key.Ascending(x => x.Email), new CreateIndexOptions { Unique = true, Name = "uq_user_email" }),
            new CreateIndexModel<User>(key.Descending(x => x.NgayTao).Ascending(x => x.MaUser), new CreateIndexOptions { Name = "created_code" }),
            new CreateIndexModel<User>(key.Ascending(x => x.VaiTro).Descending(x => x.NgayTao), new CreateIndexOptions { Name = "role_created" }),
            new CreateIndexModel<User>(key.Ascending("ThongTinMoiGioi.KhuVucHoatDong").Ascending("ThongTinMoiGioi.TrangThai"), new CreateIndexOptions { Name = "broker_area_status" })
        ]);
    }

}
