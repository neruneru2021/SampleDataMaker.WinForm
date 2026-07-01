using Microsoft.Data.SqlClient;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// SQL Serverからテーブルのカラム定義とインデックス情報を取得します。
/// </summary>
public class SqlServerDbTableSchemaRepository : IDbTableSchemaRepository
{
    public async Task<IReadOnlyList<DbColumnInfo>> GetColumnsAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.SqlServer)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は SQL Server 用Repositoryでは扱えません。");
        }

        var result = new List<DbColumnInfo>();

        await using var connection =
            new SqlConnection(connectionInfo.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                c.TABLE_SCHEMA,
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.ORDINAL_POSITION,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM sys.schemas s
                    INNER JOIN sys.tables t
                        ON t.schema_id = s.schema_id
                    INNER JOIN sys.indexes i
                        ON i.object_id = t.object_id
                    INNER JOIN sys.index_columns ic
                        ON ic.object_id = i.object_id
                        AND ic.index_id = i.index_id
                    INNER JOIN sys.columns sc
                        ON sc.object_id = ic.object_id
                        AND sc.column_id = ic.column_id
                    WHERE
                        s.name = c.TABLE_SCHEMA
                        AND t.name = c.TABLE_NAME
                        AND sc.name = c.COLUMN_NAME
                        AND i.is_hypothetical = 0
                ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IS_INDEXED,
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM sys.schemas s
                    INNER JOIN sys.tables t
                        ON t.schema_id = s.schema_id
                    INNER JOIN sys.indexes i
                        ON i.object_id = t.object_id
                    INNER JOIN sys.index_columns ic
                        ON ic.object_id = i.object_id
                        AND ic.index_id = i.index_id
                    INNER JOIN sys.columns sc
                        ON sc.object_id = ic.object_id
                        AND sc.column_id = ic.column_id
                    WHERE
                        s.name = c.TABLE_SCHEMA
                        AND t.name = c.TABLE_NAME
                        AND sc.name = c.COLUMN_NAME
                        AND i.is_unique = 1
                        AND i.is_hypothetical = 0
                ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IS_UNIQUE_INDEX
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE
                c.TABLE_SCHEMA = @schemaName
                AND c.TABLE_NAME = @tableName
            ORDER BY
                c.ORDINAL_POSITION
            """;

        command.Parameters.AddWithValue("@schemaName", table.SchemaName);
        command.Parameters.AddWithValue("@tableName", table.TableName);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DbColumnInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                ColumnName = reader.GetString(2),
                DataType = reader.GetString(3),
                IsNullable = reader.GetString(4).Equals("YES", StringComparison.OrdinalIgnoreCase),
                OrdinalPosition = reader.GetInt32(5),
                MaxLength = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                NumericPrecision = reader.IsDBNull(7) ? null : reader.GetByte(7),
                NumericScale = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                IsIndexed = reader.GetBoolean(9),
                IsUniqueIndex = reader.GetBoolean(10)
            });
        }

        return result;
    }
}
