using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using System.Data;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// DB種別に応じてテーブル情報の取得先Repositoryを切り替えます。
/// </summary>
public class CompositeDbTableInfoRepository : IDbTableInfoRepository
{
    private readonly SqlServerDbTableInfoRepository _sqlServerRepository = new();
    private readonly OracleDbTableInfoRepository _oracleRepository = new();

    public Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connection,
        CancellationToken cancellationToken = default)
    {
        return GetRepository(connection.DbType).GetTablesAsync(connection, cancellationToken);
    }

    public Task<DataTable> GetPreviewDataAsync(
        DbConnectionInfo connection,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        return GetRepository(connection.DbType).GetPreviewDataAsync(connection, table, cancellationToken);
    }

    private IDbTableInfoRepository GetRepository(DbTypeKind dbType)
    {
        return dbType switch
        {
            DbTypeKind.SqlServer => _sqlServerRepository,
            DbTypeKind.Oracle => _oracleRepository,
            _ => throw new NotSupportedException($"未対応のDB種別です: {dbType}")
        };
    }
}
