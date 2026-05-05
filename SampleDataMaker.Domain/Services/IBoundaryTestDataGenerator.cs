using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

public interface IBoundaryTestDataGenerator
{
    GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns);
}
