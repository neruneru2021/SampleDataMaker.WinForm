using System.Text.Encodings.Web;
using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

public class JsonColumnSampleDataTemplateRepository : IColumnSampleDataTemplateRepository
{
    private readonly string _directoryPath;

    public JsonColumnSampleDataTemplateRepository()
    {
        _directoryPath = Path.Combine(AppContext.BaseDirectory, "master-data");
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
