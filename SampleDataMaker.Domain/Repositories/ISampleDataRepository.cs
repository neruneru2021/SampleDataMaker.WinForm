using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface ISampleDataRepository
{
    IReadOnlyList<string> GetKinds();

    IReadOnlyList<string> GetValues(string kind);

    IReadOnlyList<SampleDataCategoryItem> GetCategoryItems();

    IReadOnlyList<SampleDataCategoryRecord> GetCategoryRecords(string categoryName);

    bool TryGetCategoryRecord(
        string categoryName,
        string recordId,
        out SampleDataCategoryRecord? record);
}
