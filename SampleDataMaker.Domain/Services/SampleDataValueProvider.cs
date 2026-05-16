using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

internal class SampleDataValueProvider
{
    private readonly IReadOnlyDictionary<string, ColumnSampleDataSetting> _settings;
    private readonly ISampleDataRepository _sampleDataRepository;

    public SampleDataValueProvider(
        IReadOnlyList<ColumnSampleDataSetting> settings,
        ISampleDataRepository sampleDataRepository)
    {
        _settings = settings
            .Where(setting => !string.IsNullOrWhiteSpace(setting.SampleDataKind))
            .GroupBy(setting => setting.ColumnName)
            .ToDictionary(group => group.Key, group => group.First());

        _sampleDataRepository = sampleDataRepository;
    }

    public string? TryCreate(DbColumnInfo column, int rowIndex)
    {
        if (!_settings.TryGetValue(column.ColumnName, out var setting))
        {
            return null;
        }

        var values = _sampleDataRepository.GetValues(setting.SampleDataKind);

        if (values.Count == 0)
        {
            return null;
        }

        return values[rowIndex % values.Count];
    }
}
