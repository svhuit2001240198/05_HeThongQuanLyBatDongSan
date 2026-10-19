using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DoAn_NoSQL_Nhom5.Services;

public sealed class SeedService(MongoContext db, IWebHostEnvironment env)
{
    public async Task RunAsync()
    {
        var folder = Path.Combine(env.ContentRootPath, "Data", "mock");
        var users = await InsertMissingAsync(Path.Combine(folder, "Users.jsonl"), db.Users, "MaUser", (User x) => x.MaUser);
        var properties = await InsertMissingAsync(Path.Combine(folder, "BDS-demo.jsonl"), db.Properties, "MaBDS", (BatDongSan x) => x.MaBDS);
        Console.WriteLine($"Đã thêm {users:N0} Users và {properties:N0} BĐS demo. Bản ghi có cùng mã được giữ nguyên.");
    }
    private static async Task<int> InsertMissingAsync<T>(string file, IMongoCollection<T> collection, string field, Func<T, string> code)
    {
        if (!File.Exists(file)) throw new FileNotFoundException("Chưa có dữ liệu mẫu. Chạy --generate-data trước.", file);
        var inserted = 0; var batch = new List<T>();
        await foreach (var line in File.ReadLinesAsync(file))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            batch.Add(BsonSerializer.Deserialize<T>(line));
            if (batch.Count == 500) { inserted += await Flush(); batch.Clear(); }
        }
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
