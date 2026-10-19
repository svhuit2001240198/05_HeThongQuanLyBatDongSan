using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Services;

public sealed class SeedService(MongoContext db, IWebHostEnvironment env)
{
    public async Task RunAsync()
    {
        var folder = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Database"));
        var users = await InsertMissingAsync(Path.Combine(folder, "DoAn_NoSQL.Users.json"), db.Users, "MaUser", (User x) => x.MaUser);
        var properties = await InsertMissingAsync(Path.Combine(folder, "DoAn_NoSQL.BDS.json"), db.Properties, "MaBDS", (BatDongSan x) => x.MaBDS);
        Console.WriteLine($"Đã thêm {users:N0} Users và {properties:N0} BĐS từ thư mục Database. Bản ghi có cùng mã được giữ nguyên.");
    }
    private static async Task<int> InsertMissingAsync<T>(string file, IMongoCollection<T> collection, string field, Func<T, string> code)
    {
        if (!File.Exists(file)) throw new FileNotFoundException("Không tìm thấy file dữ liệu mẫu trong thư mục Database.", file);
        var inserted = 0; var batch = new List<T>();
        using var textReader = File.OpenText(file);
        using var reader = new JsonReader(textReader);
        reader.ReadStartArray();
        while (reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            batch.Add(BsonSerializer.Deserialize<T>(reader));
            if (batch.Count == 500) { inserted += await Flush(); batch.Clear(); }
        }
        reader.ReadEndArray();
        if (batch.Count > 0) inserted += await Flush();
        return inserted;
        async Task<int> Flush()
        {
            var existing = await collection.Find(Builders<T>.Filter.In(field, batch.Select(code)))
                .Project<BsonDocument>(Builders<T>.Projection.Include(field).Exclude("_id")).ToListAsync();
            var known = existing.Select(x => x[field].AsString).ToHashSet();
            var missing = batch.Where(x => !known.Contains(code(x))).ToList();
            if (missing.Count > 0) await collection.InsertManyAsync(missing);
            return missing.Count;
        }
    }
}
