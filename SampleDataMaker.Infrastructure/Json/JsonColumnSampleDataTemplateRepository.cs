using System.Text.Encodings.Web;
using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

/// <summary>
/// カラム別サンプルデータ設定のテンプレートをJSONファイルで読み書きします。
/// </summary>
public class JsonColumnSampleDataTemplateRepository : IColumnSampleDataTemplateRepository
{
    private readonly string _directoryPath;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonColumnSampleDataTemplateRepository()
    {
        _directoryPath = Path.Combine(AppContext.BaseDirectory, "master-data");
    }

    public IReadOnlyList<ColumnSampleDataTemplate> GetAll()
    {
        if (!Directory.Exists(_directoryPath))
        {
            return new List<ColumnSampleDataTemplate>();
        }

        var templates = new List<ColumnSampleDataTemplate>();

        foreach (var filePath in Directory.EnumerateFiles(_directoryPath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var template = JsonSerializer.Deserialize<ColumnSampleDataTemplate>(
                    json,
                    _jsonSerializerOptions);

                if (template == null
                    || string.IsNullOrWhiteSpace(template.TableName)
                    || template.Columns.Count == 0)
                {
                    continue;
                }

                templates.Add(template);
            }
            catch (JsonException)
            {
            }
        }

        return templates
            .OrderBy(template => template.TableName)
            .ThenBy(template => template.TemplateName)
            .ToList();
    }

    public async Task SaveAsync(
        ColumnSampleDataTemplate template,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directoryPath);

        var fileName = ToSafeFileName($"{template.TableName}{template.TemplateName}");
        var filePath = Path.Combine(_directoryPath, $"{fileName}.json");
        var json = JsonSerializer.Serialize(
            template,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    private static string ToSafeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = value
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray();

        var fileName = new string(chars).Trim();

        return string.IsNullOrWhiteSpace(fileName)
            ? "template"
            : fileName;
    }
}
