using System.Text.Encodings.Web;
using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

/// <summary>
/// 外部キー設定をJSONへ保存し、旧形式の方向情報も補正して読み込みます。
/// </summary>
public class JsonForeignKeyRelationRepository : IForeignKeyRelationRepository
{
    private readonly string _filePath;

    public JsonForeignKeyRelationRepository()
        : this(Path.Combine(
            AppContext.BaseDirectory,
            "master-data",
            "foreign-key-relations.json"))
    {
    }

    public JsonForeignKeyRelationRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<ForeignKeyRelationSetting> GetAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<ForeignKeyRelationSetting>();
        }

        var json = File.ReadAllText(_filePath);

        var settings = JsonSerializer.Deserialize<List<ForeignKeyRelationSetting>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ForeignKeyRelationSetting>();

        NormalizeDirections(settings);

        return settings;
    }

    public async Task SaveAllAsync(
        IReadOnlyList<ForeignKeyRelationSetting> settings,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var normalizedSettings = settings.ToList();
        NormalizeDirections(normalizedSettings);

        var json = JsonSerializer.Serialize(
            normalizedSettings,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    private static void NormalizeDirections(IReadOnlyList<ForeignKeyRelationSetting> settings)
    {
        foreach (var group in settings.GroupBy(CreateUndirectedRelationKey))
        {
            var orderedSettings = group.ToList();
            var forward = orderedSettings.FirstOrDefault(setting => !setting.IsReverse)
                ?? orderedSettings[0];

            foreach (var setting in orderedSettings)
            {
                setting.IsReverse = !ReferenceEquals(setting, forward);
            }
        }
    }

    private static string CreateUndirectedRelationKey(ForeignKeyRelationSetting setting)
    {
        var source = CreateColumnKey(
            setting.SourceSchemaName,
            setting.SourceTableName,
            setting.SourceColumnName);
        var reference = CreateColumnKey(
            setting.ReferenceSchemaName,
            setting.ReferenceTableName,
            setting.ReferenceColumnName);

        return string.CompareOrdinal(source, reference) <= 0
            ? $"{source}|{reference}"
            : $"{reference}|{source}";
    }

    private static string CreateColumnKey(string schemaName, string tableName, string columnName)
    {
        return $"{schemaName}.{tableName}.{columnName}";
    }
}
