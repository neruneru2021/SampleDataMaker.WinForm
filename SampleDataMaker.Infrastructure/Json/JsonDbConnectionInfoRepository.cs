using System.Text.Json;
using System.Text.Json.Serialization;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

public class JsonDbConnectionInfoRepository : IDbConnectionInfoRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly string _filePath;

    public JsonDbConnectionInfoRepository()
    {
        _filePath = GetDefaultFilePath();
    }

    public JsonDbConnectionInfoRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<DbConnectionInfo> GetAll()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var settings = JsonSerializer.Deserialize<DbConnectionSettings>(json, Options);

        return settings?.Connections ?? [];
    }

    public void SaveAll(IReadOnlyList<DbConnectionInfo> connections)
    {
        var dir = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var settings = new DbConnectionSettings
        {
            Connections = connections.ToList()
        };

        var json = JsonSerializer.Serialize(settings, Options);

        File.WriteAllText(_filePath, json);
    }

    private static string GetDefaultFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "SampleDataMaker");

        return Path.Combine(dir, "connections.json");
    }

    private sealed class DbConnectionSettings
    {
        public List<DbConnectionInfo> Connections { get; set; } = new();
    }
}