using System.Data.Common;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// DB上の既存データを確認し、追加作成時にキー重複を避けるための採番開始値を取得します。
/// </summary>
public class DbExistingKeyValueRepository : IExistingKeyValueRepository
{
    private static readonly HashSet<string> NumberTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "bigint",
        "int",
        "smallint",
        "tinyint",
        "bit",
        "decimal",
        "numeric",
        "money",
        "smallmoney",
        "float",
        "real",
        "number",
        "binary_float",
        "binary_double"
    };

    /// <summary>
    /// ユニークインデックスに含まれる数値カラムの現在最大値を取得します。
    /// </summary>
    public async Task<IReadOnlyDictionary<string, int>> GetMaxValuesAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        CancellationToken cancellationToken = default)
    {
        var targetColumns = columns
            .Where(column => column.IsUniqueIndex)
            .Where(column => NumberTypes.Contains(NormalizeDataType(column.DataType)))
            .OrderBy(column => column.OrdinalPosition)
            .ToList();

        if (targetColumns.Count == 0)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        await using var connection = DbConnectionFactory.Create(connectionInfo);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = CreateCommandText(connectionInfo.DbType, table, targetColumns);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < targetColumns.Count; index++)
        {
            if (reader.IsDBNull(index))
            {
                continue;
            }

            result[CreateColumnKey(targetColumns[index])] = ConvertToInt(reader.GetValue(index));
        }

        return result;
    }

    private static string CreateCommandText(
        DbTypeKind dbType,
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns)
    {
        var selectColumns = columns
            .Select((column, index) => $"MAX({QuoteIdentifier(dbType, column.ColumnName)}) AS {QuoteIdentifier(dbType, $"c{index}")}");

        return $"""
            SELECT
                {string.Join(",\r\n                ", selectColumns)}
            FROM {CreateTableName(dbType, table)}
            """;
    }

    private static int ConvertToInt(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 1 : 0;
        }

        var decimalValue = Convert.ToDecimal(value);

        if (decimalValue >= int.MaxValue)
        {
            return int.MaxValue;
        }

        if (decimalValue <= int.MinValue)
        {
            return int.MinValue;
        }

        return (int)Math.Floor(decimalValue);
    }

    private static string CreateTableName(DbTypeKind dbType, DbTableInfo table)
    {
        return $"{QuoteIdentifier(dbType, table.SchemaName)}.{QuoteIdentifier(dbType, table.TableName)}";
    }

    private static string QuoteIdentifier(DbTypeKind dbType, string value)
    {
        return dbType switch
        {
            DbTypeKind.Oracle => $"\"{value.Replace("\"", "\"\"")}\"",
            _ => $"[{value.Replace("]", "]]")}]"
        };
    }

    private static string NormalizeDataType(string dataType)
    {
        var normalized = dataType.Trim();
        var parenthesisIndex = normalized.IndexOf('(');

        return parenthesisIndex < 0
            ? normalized
            : normalized[..parenthesisIndex].Trim();
    }

    private static string CreateColumnKey(DbColumnInfo column)
    {
        return $"{column.SchemaName}.{column.TableName}.{column.ColumnName}";
    }
}
