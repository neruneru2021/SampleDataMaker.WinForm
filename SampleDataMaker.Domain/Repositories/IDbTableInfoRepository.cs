using SampleDataMaker.Domain.Entities;
using System.Data;

namespace SampleDataMaker.Domain.Repositories;

public interface IDbTableInfoRepository
{
    Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connection,
        CancellationToken cancellationToken = default);

    Task<DataTable> GetPreviewDataAsync(
        DbConnectionInfo connection,
        DbTableInfo table,
        CancellationToken cancellationToken = default);
}
