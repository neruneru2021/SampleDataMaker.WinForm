using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using System.Text.Json;

namespace SampleDataMaker.Infrastructure.Json;

public class JsonSampleDataRepository : ISampleDataRepository
{
    private readonly string _filePath;
    private List<SampleDataItem> _items = new();

    public JsonSampleDataRepository()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "master-data", "sample-data.json");
        Load();
    }

    public IReadOnlyList<string> GetKinds()
    {
        return _items
            .Select(x => x.Kind)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    public IReadOnlyList<string> GetValues(string kind)
    {
        return _items
            .Where(x => x.Kind == kind)
            .Select(x => x.Value)
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
        _items = JsonSerializer.Deserialize<List<SampleDataItem>>(json) ?? new();
    }
}
