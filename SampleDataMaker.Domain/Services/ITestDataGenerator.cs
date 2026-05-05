using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

public interface ITestDataGenerator
{
    GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns);
}
