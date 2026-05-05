using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface IDbTableInfoRepository
{
    Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connection,
        CancellationToken cancellationToken = default);
}
