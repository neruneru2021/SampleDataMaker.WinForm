using Oracle.ManagedDataAccess.Client;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// Oracleからテーブルのカラム定義とインデックス情報を取得します。
/// </summary>
public class OracleDbTableSchemaRepository : IDbTableSchemaRepository
{
    public async Task<IReadOnlyList<DbColumnInfo>> GetColumnsAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.Oracle)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は Oracle 用Repositoryでは扱えません。");
        }

        var result = new List<DbColumnInfo>();

        await using var connection = new OracleConnection(connectionInfo.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = """
            SELECT
                c.OWNER,
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.NULLABLE,
                c.COLUMN_ID,
                c.CHAR_LENGTH,
                c.DATA_PRECISION,
                c.DATA_SCALE,
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM ALL_IND_COLUMNS ic
                    INNER JOIN ALL_INDEXES i
                        ON i.OWNER = ic.INDEX_OWNER
                        AND i.INDEX_NAME = ic.INDEX_NAME
                    WHERE
                        ic.TABLE_OWNER = c.OWNER
                        AND ic.TABLE_NAME = c.TABLE_NAME
                        AND ic.COLUMN_NAME = c.COLUMN_NAME
                ) THEN 1 ELSE 0 END AS IS_INDEXED,
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM ALL_IND_COLUMNS ic
                    INNER JOIN ALL_INDEXES i
                        ON i.OWNER = ic.INDEX_OWNER
                        AND i.INDEX_NAME = ic.INDEX_NAME
                    WHERE
                        ic.TABLE_OWNER = c.OWNER
                        AND ic.TABLE_NAME = c.TABLE_NAME
                        AND ic.COLUMN_NAME = c.COLUMN_NAME
                        AND i.UNIQUENESS = 'UNIQUE'
                ) THEN 1 ELSE 0 END AS IS_UNIQUE_INDEX
            FROM ALL_TAB_COLUMNS c
            WHERE
                c.OWNER = :schemaName
                AND c.TABLE_NAME = :tableName
            ORDER BY
                c.COLUMN_ID
            """;

        command.Parameters.Add("schemaName", OracleDbType.Varchar2).Value = table.SchemaName;
        command.Parameters.Add("tableName", OracleDbType.Varchar2).Value = table.TableName;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DbColumnInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                ColumnName = reader.GetString(2),
                DataType = reader.GetString(3),
                IsNullable = reader.GetString(4).Equals("Y", StringComparison.OrdinalIgnoreCase),
                OrdinalPosition = reader.GetInt32(5),
                MaxLength = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                NumericPrecision = reader.IsDBNull(7) ? null : Convert.ToByte(reader.GetDecimal(7)),
                NumericScale = reader.IsDBNull(8) ? null : Convert.ToInt32(reader.GetDecimal(8)),
                IsIndexed = reader.GetInt32(9) == 1,
                IsUniqueIndex = reader.GetInt32(10) == 1
            });
        }

        return result;
    }
}
