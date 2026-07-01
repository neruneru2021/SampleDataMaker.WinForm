using System.Text.Json;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Json;

/// <summary>
/// JSONファイルから通常データとカテゴリデータを読み込みます。
/// </summary>
public class JsonSampleDataRepository : ISampleDataRepository
{
    private readonly string _filePath;
    private SampleDataFile _data = new();

    public JsonSampleDataRepository()
        : this(Path.Combine(
            AppContext.BaseDirectory,
            "master-data",
            "sample-data.json"))
    {
    }

    public JsonSampleDataRepository(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    public IReadOnlyList<string> GetKinds()
    {
        return _data.SingleItems
            .Select(item => item.Kind)
            .Where(kind => !string.IsNullOrWhiteSpace(kind))
            .Distinct()
            .OrderBy(kind => kind)
            .ToList();
    }

    public IReadOnlyList<string> GetValues(string kind)
    {
        return _data.SingleItems
            .Where(item => item.Kind == kind)
            .Select(item => item.Value)
            .ToList();
    }

    public IReadOnlyList<SampleDataCategoryItem> GetCategoryItems()
    {
        return _data.Categories
            .SelectMany(category => category.Records
                .SelectMany(record => record.Values.Keys)
                .Distinct(StringComparer.Ordinal)
                .Select(itemName => new SampleDataCategoryItem(category.Name, itemName)))
            .OrderBy(item => item.CategoryName, StringComparer.Ordinal)
            .ThenBy(item => item.ItemName, StringComparer.Ordinal)
            .ToList();
    }

    public IReadOnlyList<SampleDataCategoryRecord> GetCategoryRecords(string categoryName)
    {
        var category = _data.Categories
            .FirstOrDefault(category => category.Name == categoryName);

        return category == null
            ? Array.Empty<SampleDataCategoryRecord>()
            : category.Records;
    }

    public bool TryGetCategoryRecord(
        string categoryName,
        string recordId,
        out SampleDataCategoryRecord? record)
    {
        record = GetCategoryRecords(categoryName)
            .FirstOrDefault(item => item.Id == recordId);

        return record != null;
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _data = new SampleDataFile();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _data = JsonSerializer.Deserialize<SampleDataFile>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new SampleDataFile();

        Validate();
    }

    private void Validate()
    {
        var duplicateCategory = _data.Categories
            .GroupBy(category => category.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateCategory != null)
        {
            throw new InvalidDataException(
                $"カテゴリ名 '{duplicateCategory.Key}' が重複しています。");
        }

        foreach (var category in _data.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new InvalidDataException("カテゴリ名は空にできません。");
            }

            var duplicateRecord = category.Records
                .GroupBy(record => record.Id, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateRecord != null)
            {
                throw new InvalidDataException(
                    $"カテゴリ '{category.Name}' のレコードID '{duplicateRecord.Key}' が重複しています。");
            }

            if (category.Records.Any(record => string.IsNullOrWhiteSpace(record.Id)))
            {
                throw new InvalidDataException(
                    $"カテゴリ '{category.Name}' に空のレコードIDがあります。");
            }
        }
    }
}
