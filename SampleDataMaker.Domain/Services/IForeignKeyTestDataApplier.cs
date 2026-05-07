using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

public interface IForeignKeyTestDataApplier
{
    IReadOnlyList<GeneratedTestData> Apply(
        IReadOnlyList<GeneratedTestData> testDataList,
        IReadOnlyList<ForeignKeyRelationSetting> settings);
}
