using System.Data.Common;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.Database;

public class DbTestDataDirectInsertRepository : ITestDataDirectInsertRepository
{
    public async Task<TestDataOutputResult> SaveAsync(
        DbConnectionInfo connectionInfo,
        IReadOnlyList<GeneratedTestData> testDataList,
        CancellationToken cancellationToken = default)
    {
        await using var connection = DbConnectionFactory.Create(connectionInfo);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var testData in testDataList)
            {
                foreach (var row in testData.Rows)
                {
                    await InsertRowAsync(
                        connectionInfo,
                        connection,
                        transaction,
                        testData,
                        row,
                        cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var insertedRowCount = testDataList.Sum(testData => testData.Rows.Count);

        return new TestDataOutputResult(
            $"DBへ直接登録しました。登録件数: {insertedRowCount}",
            Array.Empty<string>());
    }

    private static async Task InsertRowAsync(
        DbConnectionInfo connectionInfo,
        DbConnection connection,
        DbTransaction transaction,
        GeneratedTestData testData,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        var columns = testData.Columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();
        var columnNames = columns
            .Select(column => QuoteIdentifier(connectionInfo.DbType, column.ColumnName))
            .ToList();
        var parameterNames = columns
            .Select((_, index) => CreateParameterName(connectionInfo.DbType, index))
            .ToList();

        command.CommandText = $"""
            INSERT INTO {CreateTableName(connectionInfo.DbType, testData.Table)}
                ({string.Join(", ", columnNames)})
            VALUES
                ({string.Join(", ", parameterNames)})
            """;

        for (var index = 0; index < columns.Count; index++)
        {
            var column = columns[index];
            var parameter = command.CreateParameter();
            parameter.ParameterName = CreateDbParameterName(connectionInfo.DbType, index);
            parameter.Value = row.TryGetValue(column.ColumnName, out var value) && value != null
                ? value
                : DBNull.Value;

            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
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

    private static string CreateParameterName(DbTypeKind dbType, int index)
    {
        return dbType switch
        {
            DbTypeKind.Oracle => $":p{index}",
            _ => $"@p{index}"
        };
    }

    private static string CreateDbParameterName(DbTypeKind dbType, int index)
    {
        return dbType switch
        {
            DbTypeKind.Oracle => $"p{index}",
            _ => $"@p{index}"
        };
    }
}
