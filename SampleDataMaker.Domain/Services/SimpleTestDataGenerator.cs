using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

public class SimpleTestDataGenerator : ITestDataGenerator
{
    private readonly ITestValueFactory _testValueFactory;

    public SimpleTestDataGenerator()
        : this(new SimpleTestValueFactory())
    {
    }

    internal SimpleTestDataGenerator(ITestValueFactory testValueFactory)
    {
        _testValueFactory = testValueFactory;
    }

    public GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns)
    {
        var row = columns
            .OrderBy(column => column.OrdinalPosition)
            .ToDictionary(
                column => column.ColumnName,
                column => (string?)_testValueFactory.Create(column));

        return new GeneratedTestData(
            table,
            columns.OrderBy(column => column.OrdinalPosition).ToList(),
            new List<IReadOnlyDictionary<string, string?>> { row });
    }
}

internal interface ITestValueFactory
{
    string Create(DbColumnInfo column);
}

internal class SimpleTestValueFactory : ITestValueFactory
{
    private static readonly HashSet<string> TextTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "char",
        "nchar",
        "varchar",
        "nvarchar",
        "text",
        "ntext",
        "uniqueidentifier",
        "xml"
    };

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
        "real"
    };

    private static readonly HashSet<string> DateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "date",
        "datetime",
        "datetime2",
        "datetimeoffset",
        "smalldatetime",
        "time"
    };

    public string Create(DbColumnInfo column)
    {
        if (TextTypes.Contains(column.DataType))
        {
            return "A";
        }

        if (NumberTypes.Contains(column.DataType))
        {
            return "1";
        }

        if (DateTypes.Contains(column.DataType))
        {
            return "2026-01-01";
        }

        if (column.DataType.Equals("binary", StringComparison.OrdinalIgnoreCase)
            || column.DataType.Equals("varbinary", StringComparison.OrdinalIgnoreCase)
            || column.DataType.Equals("image", StringComparison.OrdinalIgnoreCase))
        {
            return "0x01";
        }

        return string.Empty;
    }
}
