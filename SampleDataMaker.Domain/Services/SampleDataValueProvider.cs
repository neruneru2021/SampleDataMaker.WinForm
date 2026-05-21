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
            .Where(setting => !SampleDataKindNames.IsReserved(setting.SampleDataKind))
            .GroupBy(setting => setting.ColumnName)
            .ToDictionary(group => group.Key, group => group.First());

        _sampleDataRepository = sampleDataRepository;
    }

    public bool TryCreate(DbColumnInfo column, int rowIndex, out string? value)
    {
        if (!_settings.TryGetValue(column.ColumnName, out var setting))
        {
            value = null;
            return false;
        }

        var values = _sampleDataRepository.GetValues(setting.SampleDataKind);

        if (values.Count == 0)
        {
            value = null;
            return false;
        }

        value = values[rowIndex % values.Count];
        return true;
    }
}
