using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

internal class SampleDataValueProvider
{
    private readonly IReadOnlyDictionary<string, ColumnSampleDataSetting> _settings;
    private readonly ISampleDataRepository _sampleDataRepository;
    private readonly Random _random;

    public SampleDataValueProvider(
        IReadOnlyList<ColumnSampleDataSetting> settings,
        ISampleDataRepository sampleDataRepository,
        Random? random = null)
    {
        _settings = settings
            .Where(setting => !SampleDataKindNames.IsReserved(setting.SampleDataKind))
            .GroupBy(setting => setting.ColumnName)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        _sampleDataRepository = sampleDataRepository;
        _random = random ?? new Random();
    }

    public bool TryCreate(
        DbColumnInfo column,
        int rowIndex,
        IDictionary<string, SampleDataCategoryRecord> selectedCategoryRecords,
        out string? value,
        out GeneratedColumnMetadata? metadata)
    {
        if (!_settings.TryGetValue(column.ColumnName, out var setting))
        {
            value = null;
            metadata = null;
            return false;
        }

        if (setting.IsCategory)
        {
            return TryCreateCategoryValue(
                column,
                setting,
                selectedCategoryRecords,
                out value,
                out metadata);
        }

        var values = _sampleDataRepository.GetValues(setting.SampleDataKind);

        if (values.Count == 0)
        {
            value = null;
            metadata = null;
            return false;
        }

        value = values[rowIndex % values.Count];
        metadata = null;
        return true;
    }

    private bool TryCreateCategoryValue(
        DbColumnInfo column,
        ColumnSampleDataSetting setting,
        IDictionary<string, SampleDataCategoryRecord> selectedCategoryRecords,
        out string? value,
        out GeneratedColumnMetadata? metadata)
    {
        var categoryName = setting.CategoryName!;
        var itemName = setting.CategoryItemName!;

        if (!selectedCategoryRecords.TryGetValue(categoryName, out var record))
        {
            var records = _sampleDataRepository
                .GetCategoryRecords(categoryName)
                .Where(candidate => candidate.Values.ContainsKey(itemName))
                .ToList();

            if (records.Count == 0)
            {
                throw new InvalidOperationException(
                    $"カテゴリ '{categoryName}' に項目 '{itemName}' を持つレコードがありません。");
            }

            record = records[_random.Next(records.Count)];
            selectedCategoryRecords.Add(categoryName, record);
        }

        if (!record.Values.TryGetValue(itemName, out var categoryValue))
        {
            throw new InvalidOperationException(
                $"カテゴリ '{categoryName}' のレコード '{record.Id}' に項目 '{itemName}' がありません。");
        }

        value = categoryValue;
        metadata = new GeneratedColumnMetadata(
            column.ColumnName,
            categoryName,
            itemName,
            record.Id);

        return true;
    }
}
