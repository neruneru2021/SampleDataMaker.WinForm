using System.Text;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.FileSystem;

/// <summary>
/// 生成したテストデータをCSV、Markdown、SQLファイルとして保存します。
/// </summary>
public class LocalTestDataOutputRepository : ITestDataOutputRepository
{
    public async Task<TestDataOutputResult> SaveAsync(
        IReadOnlyList<GeneratedTestData> testDataList,
        CancellationToken cancellationToken = default)
    {
        var outputDirectoryPath = Path.Combine(
            AppContext.BaseDirectory,
            DateTime.Now.ToString("yyyyMMdd"));

        Directory.CreateDirectory(outputDirectoryPath);

        var filePaths = new List<string>();

        foreach (var testData in testDataList)
        {
            var fileName = ToSafeFileName(testData.Table.TableName);
            var csvPath = Path.Combine(outputDirectoryPath, $"{fileName}.csv");
            var markdownPath = Path.Combine(outputDirectoryPath, $"{fileName}.md");
            var sqlPath = Path.Combine(outputDirectoryPath, $"{fileName}.sql");

            await File.WriteAllTextAsync(
                csvPath,
                CreateCsv(testData),
                Encoding.UTF8,
                cancellationToken);

            await File.WriteAllTextAsync(
                markdownPath,
                CreateMarkdown(testData),
                Encoding.UTF8,
                cancellationToken);

            await File.WriteAllTextAsync(
                sqlPath,
                CreateSql(testData),
                Encoding.UTF8,
                cancellationToken);

            filePaths.Add(csvPath);
            filePaths.Add(markdownPath);
            filePaths.Add(sqlPath);
        }

        return new TestDataOutputResult(outputDirectoryPath, filePaths);
    }

    private static string CreateCsv(GeneratedTestData testData)
    {
        var builder = new StringBuilder();
        var columnNames = testData.Columns.Select(column => column.ColumnName).ToList();

        builder.AppendLine(string.Join(",", columnNames.Select(EscapeCsv)));

        foreach (var row in testData.Rows)
        {
            builder.AppendLine(string.Join(
                ",",
                columnNames.Select(columnName =>
                    EscapeCsv(row.TryGetValue(columnName, out var value) ? value : string.Empty))));
        }

        return builder.ToString();
    }

    private static string CreateSql(GeneratedTestData testData)
    {
        var builder = new StringBuilder();
        var columnNames = testData.Columns.Select(column => column.ColumnName).ToList();
        var tableName = ToSqlTableName(testData.Table);
        var columnList = string.Join(", ", columnNames.Select(ToSqlIdentifier));

        foreach (var row in testData.Rows)
        {
            var values = string.Join(
                ", ",
                columnNames.Select(columnName =>
                    ToSqlValue(row.TryGetValue(columnName, out var value) ? value : string.Empty)));

            builder.AppendLine($"INSERT INTO {tableName} ({columnList}) VALUES ({values});");
        }

        return builder.ToString();
    }

    private static string CreateMarkdown(GeneratedTestData testData)
    {
        var builder = new StringBuilder();
        var columnNames = testData.Columns.Select(column => column.ColumnName).ToList();

        builder.AppendLine($"# {testData.Table.DisplayName}");
        builder.AppendLine();
        builder.AppendLine("## Columns");
        builder.AppendLine();
        builder.AppendLine("| Column | DataType | Nullable |");
        builder.AppendLine("| --- | --- | --- |");

        foreach (var column in testData.Columns)
        {
            builder.AppendLine(
                $"| {EscapeMarkdown(column.ColumnName)} | {EscapeMarkdown(column.DataType)} | {(column.IsNullable ? "YES" : "NO")} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Test Data");
        builder.AppendLine();
        builder.AppendLine($"| {string.Join(" | ", columnNames.Select(EscapeMarkdown))} |");
        builder.AppendLine($"| {string.Join(" | ", columnNames.Select(_ => "---"))} |");

        foreach (var row in testData.Rows)
        {
            builder.AppendLine(
                $"| {string.Join(" | ", columnNames.Select(columnName => EscapeMarkdown(row.TryGetValue(columnName, out var value) ? value : string.Empty)))} |");
        }

        return builder.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (value == null)
        {
            return "NULL";
        }

        if (value.Length == 0)
        {
            return "\"\"";
        }

        if (!value.Contains('"') && !value.Contains(',') && !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string EscapeMarkdown(string? value)
    {
        if (value == null)
        {
            return "NULL";
        }

        if (value.Length == 0)
        {
            return "''";
        }

        return value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }

    private static string ToSqlTableName(DbTableInfo table)
    {
        return $"{ToSqlIdentifier(table.SchemaName)}.{ToSqlIdentifier(table.TableName)}";
    }

    private static string ToSqlIdentifier(string value)
    {
        return $"[{value.Replace("]", "]]")}]";
    }

    private static string ToSqlValue(string? value)
    {
        if (value == null)
        {
            return "NULL";
        }

        return $"N'{value.Replace("'", "''")}'";
    }

    private static string ToSafeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = value
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray();

        var fileName = new string(chars).Trim();

        return string.IsNullOrWhiteSpace(fileName)
            ? "table"
            : fileName;
    }
}
