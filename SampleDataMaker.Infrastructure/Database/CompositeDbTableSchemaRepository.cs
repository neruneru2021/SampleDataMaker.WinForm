using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// DB種別に応じてカラム定義の取得先Repositoryを切り替えます。
/// </summary>
public class CompositeDbTableSchemaRepository : IDbTableSchemaRepository
{
    private readonly SqlServerDbTableSchemaRepository _sqlServerRepository = new();
    private readonly OracleDbTableSchemaRepository _oracleRepository = new();

    public Task<IReadOnlyList<DbColumnInfo>> GetColumnsAsync(
        DbConnectionInfo connection,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        return GetRepository(connection.DbType).GetColumnsAsync(connection, table, cancellationToken);
    }

    private IDbTableSchemaRepository GetRepository(DbTypeKind dbType)
    {
        return dbType switch
        {
            DbTypeKind.SqlServer => _sqlServerRepository,
            DbTypeKind.Oracle => _oracleRepository,
            _ => throw new NotSupportedException($"未対応のDB種別です: {dbType}")
        };
    }
}
