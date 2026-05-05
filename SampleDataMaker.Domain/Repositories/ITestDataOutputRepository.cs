using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface ITestDataOutputRepository
{
    Task<TestDataOutputResult> SaveAsync(
        IReadOnlyList<GeneratedTestData> testDataList,
        CancellationToken cancellationToken = default);
}
