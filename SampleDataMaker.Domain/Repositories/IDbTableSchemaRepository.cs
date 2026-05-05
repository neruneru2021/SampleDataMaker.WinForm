using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface IDbTableSchemaRepository
{
    Task<IReadOnlyList<DbColumnInfo>> GetColumnsAsync(
        DbConnectionInfo connection,
        DbTableInfo table,
        CancellationToken cancellationToken = default);
}
