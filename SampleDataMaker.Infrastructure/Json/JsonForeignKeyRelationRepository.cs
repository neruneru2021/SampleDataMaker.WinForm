using System.Text.Encodings.Web;
using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

public class JsonForeignKeyRelationRepository : IForeignKeyRelationRepository
{
    private readonly string _filePath;

    public JsonForeignKeyRelationRepository()
    {
        _filePath = Path.Combine(
            AppContext.BaseDirectory,
            "master-data",
            "foreign-key-relations.json");
    }

    public IReadOnlyList<ForeignKeyRelationSetting> GetAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<ForeignKeyRelationSetting>();
        }

        var json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<List<ForeignKeyRelationSetting>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ForeignKeyRelationSetting>();
    }

    public async Task SaveAllAsync(
        IReadOnlyList<ForeignKeyRelationSetting> settings,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }
}
