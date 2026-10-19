using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DoAn_NoSQL_Nhom5.Infrastructure;
using DoAn_NoSQL_Nhom5.ViewModels;
using Microsoft.Extensions.Options;

namespace DoAn_NoSQL_Nhom5.Services;

// Client RESP2 cho Redis local của đồ án. Không dùng TLS, cluster hay Pub/Sub.
public sealed class RedisHistoryService(IOptions<RedisSettings> options, ILogger<RedisHistoryService> logger)
{
    private long _lastWarning;
    private readonly RedisSettings _config = options.Value;
    public async Task TrackAsync(string? userCode, string propertyCode)
    {
        var commands = new List<string[]> { new[] { "ZINCRBY", "bds:hot", "1", propertyCode }, new[] { "DEL", "cache:bds:hot" } };
        if (userCode is not null)
        {
            var key = "history:user:" + userCode;
            commands.Add(["LPUSH", key, JsonSerializer.Serialize(new ViewHistoryItem { MaBDS = propertyCode, ViewedAt = DateTime.UtcNow })]);
            commands.Add(["LTRIM", key, "0", "29"]); commands.Add(["EXPIRE", key, "2592000"]);
        }
        await ExecuteAsync(commands.ToArray());
    }
    public async Task<(bool Available, List<ViewHistoryItem> Items)> HistoryAsync(string code)
    {
        var result = await ExecuteAsync(new[] { "LRANGE", "history:user:" + code, "0", "29" });
        if (!result.Available) return (false, []);
        var items = new List<ViewHistoryItem>();
        foreach (var raw in result.Values[0] as object?[] ?? [])
        {
            if (raw is not string json) continue;
            try { var item = JsonSerializer.Deserialize<ViewHistoryItem>(json); if (item is not null) items.Add(item); }
            catch (JsonException) { }
        }
        return (true, items.DistinctBy(x => x.MaBDS).ToList());
    }
    public Task ClearAsync(string code) => ExecuteAsync(new[] { "DEL", "history:user:" + code });
    public async Task<List<string>> HotCodesAsync()
    {
        var cache = await ExecuteAsync(new[] { "GET", "cache:bds:hot" });
        if (cache.Available && cache.Values[0] is string json)
        { try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; } catch (JsonException) { } }
        var hot = await ExecuteAsync(new[] { "ZREVRANGE", "bds:hot", "0", "5" });
        if (!hot.Available) return [];
        var codes = (hot.Values[0] as object?[] ?? []).OfType<string>().ToList();
        await ExecuteAsync(new[] { "SET", "cache:bds:hot", JsonSerializer.Serialize(codes), "EX", "60" });
        return codes;
    }
    private async Task<(bool Available, List<object?> Values)> ExecuteAsync(params string[][] commands)
    {
        if (!_config.Enabled) return (false, []);
        using var timeout = new CancellationTokenSource(Math.Clamp(_config.TimeoutMs, 300, 5000)); var ct = timeout.Token;
        try
        {
            using var client = new TcpClient(); await client.ConnectAsync(_config.Host, _config.Port, ct);
            await using var stream = client.GetStream();
            if (_config.Password.Length > 0) { await WriteAsync(stream, ["AUTH", _config.Password], ct); await ReadAsync(stream, ct); }
            if (_config.Database != 0) { await WriteAsync(stream, ["SELECT", _config.Database.ToString(CultureInfo.InvariantCulture)], ct); await ReadAsync(stream, ct); }
            var values = new List<object?>();
            foreach (var command in commands) { await WriteAsync(stream, command, ct); values.Add(await ReadAsync(stream, ct)); }
            return (true, values);
        }
        catch (Exception ex) when (ex is SocketException or IOException or OperationCanceledException)
        {
            var now = DateTime.UtcNow.Ticks;
            if (now - Interlocked.Read(ref _lastWarning) > TimeSpan.FromMinutes(1).Ticks)
            { Interlocked.Exchange(ref _lastWarning, now); logger.LogWarning("Redis không khả dụng: {Reason}", ex.Message); }
            return (false, []);
        }
    }
    private static async Task WriteAsync(Stream stream, string[] command, CancellationToken ct)
    {
        using var bytes = new MemoryStream(); bytes.Write(Encoding.UTF8.GetBytes("*" + command.Length + "\r\n"));
        foreach (var value in command)
        {
            var data = Encoding.UTF8.GetBytes(value);
            bytes.Write(Encoding.UTF8.GetBytes("$" + data.Length + "\r\n")); bytes.Write(data); bytes.Write(new byte[] { 13, 10 });
        }
        await stream.WriteAsync(bytes.ToArray(), ct);
    }
    private static async Task<object?> ReadAsync(Stream stream, CancellationToken ct)
    {
        var first = new byte[1]; await stream.ReadExactlyAsync(first, ct); var line = await ReadLineAsync(stream, ct);
        switch ((char)first[0])
        {
            case '+': return line;
            case '-': throw new InvalidDataException(line);
            case ':': return long.Parse(line, CultureInfo.InvariantCulture);
            case '$':
                var len = int.Parse(line, CultureInfo.InvariantCulture);
                if (len == -1) return null;
                if (len < 0 || len > 2_000_000) throw new InvalidDataException("Redis bulk length không hợp lệ.");
                var data = new byte[len]; await stream.ReadExactlyAsync(data, ct);
                var end = new byte[2]; await stream.ReadExactlyAsync(end, ct);
                if (end[0] != 13 || end[1] != 10) throw new InvalidDataException("Redis frame không hợp lệ.");
                return Encoding.UTF8.GetString(data);
            case '*':
                var count = int.Parse(line, CultureInfo.InvariantCulture);
                if (count == -1) return null;
                if (count < 0 || count > 10000) throw new InvalidDataException("Redis array length không hợp lệ.");
                var array = new object?[count]; for (var i = 0; i < count; i++) array[i] = await ReadAsync(stream, ct);
                return array;
            default: throw new InvalidDataException("Redis response không hỗ trợ.");
        }
    }
    private static async Task<string> ReadLineAsync(Stream stream, CancellationToken ct)
    {
        using var buffer = new MemoryStream(); var one = new byte[1];
        while (buffer.Length <= 65536)
        {
            await stream.ReadExactlyAsync(one, ct);
            if (one[0] == 13) { await stream.ReadExactlyAsync(one, ct); if (one[0] != 10) throw new InvalidDataException("Redis line không hợp lệ."); return Encoding.UTF8.GetString(buffer.ToArray()); }
            buffer.WriteByte(one[0]);
        }
        throw new InvalidDataException("Redis line quá dài.");
    }
}
