namespace DoAn_NoSQL_Nhom5.Infrastructure;

public sealed class MongoDBSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "DoAn_NoSQL";
    public string UsersCollection { get; set; } = "Users";
    public string BDSCollection { get; set; } = "BDS";
}

public sealed class RedisSettings
{
    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = "";
    public int Database { get; set; }
    public int TimeoutMs { get; set; } = 1500;
}

public static class UserRoles
{
    public const string Buyer = "nguoi_mua";
    public const string Seller = "nguoi_ban";
    public const string Broker = "moi_gioi";
    public const string Admin = "admin";
    public static readonly string[] PublicRoles = [Buyer, Seller, Broker];
    public static readonly string[] All = [Buyer, Seller, Broker, Admin];
}
