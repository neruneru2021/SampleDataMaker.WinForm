using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

public class JsonSampleDataRepository : ISampleDataRepository
{
    private readonly string _filePath;
    private List<SampleDataItem> _items = new();

    public JsonSampleDataRepository()
    {
        _filePath = Path.Combine(
            AppContext.BaseDirectory,
            "master-data",
            "sample-data.json");

        Load();
    }

    public IReadOnlyList<string> GetKinds()
    {
        return _items
            .Select(item => item.Kind)
            .Where(kind => !string.IsNullOrWhiteSpace(kind))
            .Distinct()
            .OrderBy(kind => kind)
            .ToList();
    }

    public IReadOnlyList<string> GetValues(string kind)
    {
        return _items
            .Where(item => item.Kind == kind)
            .Select(item => item.Value)
            .ToList();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _items = new List<SampleDataItem>();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _items = JsonSerializer.Deserialize<List<SampleDataItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<SampleDataItem>();
    }
}
