using Microsoft.Data.SqlClient;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

public class SqlServerDbTableInfoRepository : IDbTableInfoRepository
{
    public async Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connectionInfo,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.SqlServer)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は SQL Server 用Repositoryでは扱えません。");
        }

        var result = new List<DbTableInfo>();

        await using var connection =
            new SqlConnection(connectionInfo.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY
                TABLE_SCHEMA,
                TABLE_NAME
            """;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DbTableInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
            });
        }

        return result;
    }
}
