using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using System.Data.Common;

namespace SampleDataMaker.WinForm.Services;

/// <summary>
/// 接続情報と操作対象から、画面タイトルに表示する接続先概要を作成します。
/// </summary>
internal static class ConnectionTitleFormatter
{
    /// <summary>
    /// テーブル操作画面向けのタイトルを作成します。
    /// </summary>
    internal static string CreateOperationTitle(DbConnectionInfo connection)
    {
        var summary = CreateConnectionSummary(connection);

        return string.IsNullOrWhiteSpace(summary)
            ? $"テーブル操作 - {connection.DbType}"
            : $"テーブル操作 - {connection.DbType} | {summary}";
    }

    /// <summary>
    /// 外部キー選択画面向けに、設定元カラムを含めたタイトルを作成します。
    /// </summary>
    internal static string CreateForeignKeyTitle(
        DbConnectionInfo connection,
        DbColumnInfo sourceColumn)
    {
        var summary = CreateConnectionSummary(connection);
        var source = $"{sourceColumn.SchemaName}.{sourceColumn.TableName}.{sourceColumn.ColumnName}";

        return string.IsNullOrWhiteSpace(summary)
            ? $"外部キー設定 - {connection.DbType} | Source={source}"
            : $"外部キー設定 - {connection.DbType} | {summary} | Source={source}";
    }

    /// <summary>
    /// DB種別と接続文字列から、利用者が接続先を判別しやすい概要を作成します。
    /// </summary>
    private static string CreateConnectionSummary(DbConnectionInfo connection)
    {
        return connection.DbType switch
        {
            DbTypeKind.Oracle => CreateOracleConnectionSummary(connection),
            DbTypeKind.SqlServer => CreateSqlServerConnectionSummary(connection),
            _ => connection.ConnectionString
        };
    }

    /// <summary>
    /// Oracle接続文字列からホスト、ポート、サービス名、既定スキーマを抜き出します。
    /// </summary>
    private static string CreateOracleConnectionSummary(DbConnectionInfo connection)
    {
        var dataSource = GetConnectionStringValue(connection.ConnectionString, "Data Source");
        var schema = connection.DefaultSchema;

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            return string.IsNullOrWhiteSpace(schema)
                ? string.Empty
                : $"Schema={schema}";
        }

        var host = dataSource;
        var serviceName = string.Empty;
        var slashIndex = dataSource.LastIndexOf('/');

        if (slashIndex >= 0)
        {
            host = dataSource[..slashIndex];
            serviceName = dataSource[(slashIndex + 1)..];
        }

        var parts = new List<string> { $"Host={host}" };

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            parts.Add($"Service={serviceName}");
        }

        if (!string.IsNullOrWhiteSpace(schema))
        {
            parts.Add($"Schema={schema}");
        }

        return string.Join(" / ", parts);
    }

    /// <summary>
    /// SQL Server接続文字列からホスト、インスタンス、DB名を抜き出します。
    /// </summary>
    private static string CreateSqlServerConnectionSummary(DbConnectionInfo connection)
    {
        var server = GetConnectionStringValue(
            connection.ConnectionString,
            "Server",
            "Data Source",
            "Address",
            "Addr",
            "Network Address");
        var database = GetConnectionStringValue(
            connection.ConnectionString,
            "Database",
            "Initial Catalog");

        if (string.IsNullOrWhiteSpace(server) && string.IsNullOrWhiteSpace(database))
        {
            return string.Empty;
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(server))
        {
            parts.Add($"Host={server}");
        }

        if (!string.IsNullOrWhiteSpace(database))
        {
            parts.Add($"Database={database}");
        }

        return string.Join(" / ", parts);
    }

    /// <summary>
    /// 接続文字列から指定されたキーの値を取得します。
    /// </summary>
    private static string GetConnectionStringValue(string connectionString, params string[] keys)
    {
        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            foreach (var key in keys)
            {
                if (builder.TryGetValue(key, out var value))
                {
                    return Convert.ToString(value) ?? string.Empty;
                }
            }
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }

        return string.Empty;
    }
}
