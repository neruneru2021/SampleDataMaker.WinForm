using Oracle.ManagedDataAccess.Client;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using System.Data;

namespace SampleDataMaker.Infrastructure.Database;

public class OracleDbTableInfoRepository : IDbTableInfoRepository
{
    public async Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connectionInfo,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.Oracle)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は Oracle 用Repositoryでは扱えません。");
        }

        var result = new List<DbTableInfo>();

        await using var connection = new OracleConnection(connectionInfo.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = """
            SELECT
                OWNER,
                TABLE_NAME
            FROM ALL_TABLES
            WHERE
                OWNER = :schemaName
                AND NESTED = 'NO'
            ORDER BY
                OWNER,
                TABLE_NAME
            """;

        command.Parameters.Add("schemaName", OracleDbType.Varchar2).Value =
            GetSchemaName(connectionInfo);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DbTableInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1)
            });
        }

        return result;
    }

    public async Task<DataTable> GetPreviewDataAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.Oracle)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は Oracle 用Repositoryでは扱えません。");
        }

        await using var connection = new OracleConnection(connectionInfo.ConnectionString);
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
        command.BindByName = true;
        command.CommandText = $"""
            SELECT
                {columnList}
            FROM
            (
                SELECT
                    base_rows.*,
                    ROW_NUMBER() OVER (ORDER BY NULL) AS {QuoteIdentifier("__SampleDataMakerRowNumber")}
                FROM {quotedTableName} base_rows
            ) src
            WHERE
                src.{QuoteIdentifier("__SampleDataMakerRowNumber")} <= 10
                OR src.{QuoteIdentifier("__SampleDataMakerRowNumber")} BETWEEN :middleStart AND :middleEnd
                OR src.{QuoteIdentifier("__SampleDataMakerRowNumber")} >= :tailStart
            ORDER BY
                src.{QuoteIdentifier("__SampleDataMakerRowNumber")}
            """;

        command.Parameters.Add("middleStart", OracleDbType.Int64).Value = middleStart;
        command.Parameters.Add("middleEnd", OracleDbType.Int64).Value = middleEnd;
        command.Parameters.Add("tailStart", OracleDbType.Int64).Value = tailStart;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        result.Clear();
        result.Load(reader);

        return result;
    }

    private static async Task<DataTable> LoadEmptyTableAsync(
        OracleConnection connection,
        string quotedTableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT
                *
            FROM {quotedTableName}
            WHERE 1 = 0
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new DataTable();
        result.Load(reader);

        return result;
    }

    private static async Task<long> GetTableRowCountAsync(
        OracleConnection connection,
        string quotedTableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT
                COUNT(*)
            FROM {quotedTableName}
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(result);
    }

    private static string GetSchemaName(DbConnectionInfo connectionInfo)
    {
        return string.IsNullOrWhiteSpace(connectionInfo.DefaultSchema)
            ? GetUserId(connectionInfo).ToUpperInvariant()
            : connectionInfo.DefaultSchema.Trim().ToUpperInvariant();
    }

    private static string GetUserId(DbConnectionInfo connectionInfo)
    {
        var builder = new OracleConnectionStringBuilder(connectionInfo.ConnectionString);

        return builder.UserID;
    }

    private static string CreateQuotedTableName(DbTableInfo table)
    {
        return $"{QuoteIdentifier(table.SchemaName)}.{QuoteIdentifier(table.TableName)}";
    }

    private static string QuoteIdentifier(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
