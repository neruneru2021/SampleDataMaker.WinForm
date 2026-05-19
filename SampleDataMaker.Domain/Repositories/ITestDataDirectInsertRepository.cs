using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface ITestDataDirectInsertRepository
{
    Task<TestDataOutputResult> SaveAsync(
        DbConnectionInfo connection,
        IReadOnlyList<GeneratedTestData> testDataList,
        CancellationToken cancellationToken = default);
}
