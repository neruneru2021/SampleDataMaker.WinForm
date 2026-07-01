using Microsoft.Data.SqlClient;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using System.Data;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// SQL Serverからテーブル一覧とプレビュー用データを取得します。
/// </summary>
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

    public async Task<DataTable> GetPreviewDataAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.SqlServer)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は SQL Server 用Repositoryでは扱えません。");
        }

        await using var connection =
            new SqlConnection(connectionInfo.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        var quotedTableName = CreateQuotedTableName(table);
        var result = await LoadEmptyTableAsync(connection, quotedTableName, cancellationToken);
        var totalRows = await GetTableRowCountAsync(connection, quotedTableName, cancellationToken);

        if (totalRows == 0)
        {
            return result;
        }

        var middleStart = Math.Max(1, ((totalRows - 10) / 2) + 1);
        var middleEnd = middleStart + 9;
        var tailStart = Math.Max(1, totalRows - 9);
        var columnList = string.Join(
            ", ",
            result.Columns
                .Cast<DataColumn>()
                .Select(column => $"src.{QuoteIdentifier(column.ColumnName)}"));

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            WITH SourceRows AS
            (
                SELECT
                    *,
                    ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS {QuoteIdentifier("__SampleDataMakerRowNumber")}
                FROM {quotedTableName}
            )
            SELECT
                {columnList}
            FROM SourceRows src
            WHERE
                src.{QuoteIdentifier("__SampleDataMakerRowNumber")} <= 10
                OR src.{QuoteIdentifier("__SampleDataMakerRowNumber")} BETWEEN @middleStart AND @middleEnd
                OR src.{QuoteIdentifier("__SampleDataMakerRowNumber")} >= @tailStart
            ORDER BY
                src.{QuoteIdentifier("__SampleDataMakerRowNumber")}
            """;

        command.Parameters.AddWithValue("@middleStart", middleStart);
        command.Parameters.AddWithValue("@middleEnd", middleEnd);
        command.Parameters.AddWithValue("@tailStart", tailStart);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        result.Clear();
        result.Load(reader);

        return result;
    }

    private static async Task<DataTable> LoadEmptyTableAsync(
        SqlConnection connection,
        string quotedTableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            SELECT TOP (0)
                *
            FROM {quotedTableName}
            """;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new DataTable();
        result.Load(reader);

        return result;
    }

    private static async Task<long> GetTableRowCountAsync(
        SqlConnection connection,
        string quotedTableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            SELECT
                COUNT_BIG(*)
            FROM {quotedTableName}
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(result);
    }

    private static string CreateQuotedTableName(DbTableInfo table)
    {
        return $"{QuoteIdentifier(table.SchemaName)}.{QuoteIdentifier(table.TableName)}";
    }

    private static string QuoteIdentifier(string value)
    {
        return $"[{value.Replace("]", "]]")}]";
    }
}
