using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

public class SimpleTestDataGenerator : ITestDataGenerator
{
    private readonly ITestValueFactory _testValueFactory;
    private readonly ISampleDataRepository _sampleDataRepository;

    public SimpleTestDataGenerator(ISampleDataRepository sampleDataRepository)
        : this(new SimpleTestValueFactory(), sampleDataRepository)
    {
    }

    internal SimpleTestDataGenerator(
        ITestValueFactory testValueFactory,
        ISampleDataRepository sampleDataRepository)
    {
        _testValueFactory = testValueFactory;
        _sampleDataRepository = sampleDataRepository;
    }

    public GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<ColumnSampleDataSetting>? sampleDataSettings = null,
        int rowCount = 1)
    {
        var sampleProvider = new SampleDataValueProvider(
            sampleDataSettings ?? Array.Empty<ColumnSampleDataSetting>(),
            _sampleDataRepository);
        var orderedColumns = columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();
        var rows = new List<IReadOnlyDictionary<string, string?>>();

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            IReadOnlyDictionary<string, string?> row = orderedColumns
                .ToDictionary<DbColumnInfo, string, string?>(
                    column => column.ColumnName,
                    column => sampleProvider.TryCreate(column, rowIndex) ?? _testValueFactory.Create(column));

            rows.Add(row);
        }

        return new GeneratedTestData(
            table,
            orderedColumns,
            rows);
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
