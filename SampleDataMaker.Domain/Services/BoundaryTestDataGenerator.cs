using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

public class BoundaryTestDataGenerator : IBoundaryTestDataGenerator
{
    private readonly BoundaryTestValueFactory _valueFactory = new();
    private readonly ISampleDataRepository _sampleDataRepository;

    public BoundaryTestDataGenerator(ISampleDataRepository sampleDataRepository)
    {
        _sampleDataRepository = sampleDataRepository;
    }

    public GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<ColumnSampleDataSetting>? sampleDataSettings = null)
    {
        var orderedColumns = columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();
        var sampleProvider = new SampleDataValueProvider(
            sampleDataSettings ?? Array.Empty<ColumnSampleDataSetting>(),
            _sampleDataRepository);

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        var rowNumber = 1;

        foreach (var column in orderedColumns)
        {
            rows.Add(CreateRow(orderedColumns, column, _valueFactory.CreateMinimum(column), rowNumber++, sampleProvider));
            rows.Add(CreateRow(orderedColumns, column, _valueFactory.CreateMaximum(column), rowNumber++, sampleProvider));

            if (column.IsNullable)
            {
                rows.Add(CreateRow(orderedColumns, column, null, rowNumber++, sampleProvider));
            }

            if (_valueFactory.CanUseEmptyString(column))
            {
                rows.Add(CreateRow(orderedColumns, column, string.Empty, rowNumber++, sampleProvider));
            }
        }

        return new GeneratedTestData(table, orderedColumns, rows);
    }

    private IReadOnlyDictionary<string, string?> CreateRow(
        IReadOnlyList<DbColumnInfo> columns,
        DbColumnInfo targetColumn,
        string? targetValue,
        int rowNumber,
        SampleDataValueProvider sampleProvider)
    {
        return columns.ToDictionary(
            column => column.ColumnName,
            column =>
            {
                if (column.ColumnName == targetColumn.ColumnName)
                {
                    return targetValue;
                }

                if (column.IsIndexed)
                {
                    return _valueFactory.CreateUnique(column, rowNumber);
                }

                if (sampleProvider.TryCreate(column, rowNumber - 1, out var sampleValue))
                {
                    return sampleValue;
                }

                return _valueFactory.CreateDefault(column);
            });
    }
}

internal class BoundaryTestValueFactory
{
    private static readonly HashSet<string> TextTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "char",
        "nchar",
        "varchar",
        "nvarchar",
        "varchar2",
        "nvarchar2",
        "text",
        "ntext",
        "clob",
        "nclob",
        "uniqueidentifier",
        "xml"
    };

    private static readonly HashSet<string> IntegerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "bigint",
        "int",
        "smallint",
        "tinyint"
    };

    private static readonly HashSet<string> DecimalTypes = new(StringComparer.OrdinalIgnoreCase)
    {
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

    private static readonly HashSet<string> DateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "date",
        "datetime",
        "datetime2",
        "datetimeoffset",
        "smalldatetime",
        "time",
        "timestamp",
        "timestamp with time zone",
        "timestamp with local time zone"
    };

    public string CreateDefault(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        if (TextTypes.Contains(dataType))
        {
            return "A";
        }

        if (IntegerTypes.Contains(dataType)
            || DecimalTypes.Contains(dataType)
            || dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
        {
            return "1";
        }

        if (DateTypes.Contains(dataType))
        {
            return "2026-01-01";
        }

        if (IsBinary(column))
        {
            return "0x01";
        }

        return string.Empty;
    }

    public string CreateUnique(DbColumnInfo column, int rowNumber)
    {
        var dataType = NormalizeDataType(column.DataType);

        if (TextTypes.Contains(dataType))
        {
            return $"A{rowNumber}";
        }

        if (dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
        {
            return (rowNumber % 2).ToString();
        }

        if (IntegerTypes.Contains(dataType) || DecimalTypes.Contains(dataType))
        {
            return rowNumber.ToString();
        }

        if (DateTypes.Contains(dataType))
        {
            return new DateTime(2026, 1, 1).AddDays(rowNumber).ToString("yyyy-MM-dd");
        }

        if (IsBinary(column))
        {
            return $"0x{rowNumber:X2}";
        }

        return rowNumber.ToString();
    }

    public string CreateMinimum(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        return dataType.ToLowerInvariant() switch
        {
            "bigint" => long.MinValue.ToString(),
            "int" => int.MinValue.ToString(),
            "smallint" => short.MinValue.ToString(),
            "tinyint" => byte.MinValue.ToString(),
            "bit" => "0",
            "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real"
                or "number" or "binary_float" or "binary_double" => "0",
            "date" or "datetime" or "datetime2" or "datetimeoffset" or "smalldatetime" => "1900-01-01",
            "timestamp" or "timestamp with time zone" or "timestamp with local time zone" => "1900-01-01",
            "time" => "00:00:00",
            _ when TextTypes.Contains(dataType) => CanUseEmptyString(column) ? string.Empty : "A",
            _ when IsBinary(column) => "0x00",
            _ => CreateDefault(column)
        };
    }

    public string CreateMaximum(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        return dataType.ToLowerInvariant() switch
        {
            "bigint" => long.MaxValue.ToString(),
            "int" => int.MaxValue.ToString(),
            "smallint" => short.MaxValue.ToString(),
            "tinyint" => byte.MaxValue.ToString(),
            "bit" => "1",
            "decimal" or "numeric" or "number" => CreateNumericMaximum(column),
            "money" => "922337203685477.5807",
            "smallmoney" => "214748.3647",
            "float" or "real" or "binary_float" or "binary_double" => "1",
            "date" => "9999-12-31",
            "datetime" or "smalldatetime" => "9999-12-31",
            "datetime2" or "datetimeoffset" => "9999-12-31",
            "timestamp" or "timestamp with time zone" or "timestamp with local time zone" => "9999-12-31",
            "time" => "23:59:59",
            _ when TextTypes.Contains(dataType) => CreateTextMaximum(column),
            _ when IsBinary(column) => "0xFF",
            _ => CreateDefault(column)
        };
    }

    public bool CanUseEmptyString(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        return TextTypes.Contains(dataType)
            && dataType.IndexOf("uniqueidentifier", StringComparison.OrdinalIgnoreCase) < 0
            && dataType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static string CreateNumericMaximum(DbColumnInfo column)
    {
        var precision = column.NumericPrecision.GetValueOrDefault(9);
        var scale = column.NumericScale.GetValueOrDefault(0);
        var integerDigits = Math.Max(1, precision - scale);
        var integerPart = new string('9', integerDigits);

        if (scale == 0)
        {
            return integerPart;
        }

        return $"{integerPart}.{new string('9', scale)}";
    }

    private static string CreateTextMaximum(DbColumnInfo column)
    {
        var length = column.MaxLength.GetValueOrDefault(10);

        if (length <= 0 || length > 100)
        {
            length = 100;
        }

        if (column.DataType.StartsWith("n", StringComparison.OrdinalIgnoreCase))
        {
            length = Math.Max(1, length / 2);
        }

        return new string('Z', length);
    }

    private static bool IsBinary(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        return dataType.Equals("binary", StringComparison.OrdinalIgnoreCase)
            || dataType.Equals("varbinary", StringComparison.OrdinalIgnoreCase)
            || dataType.Equals("image", StringComparison.OrdinalIgnoreCase)
            || dataType.Equals("raw", StringComparison.OrdinalIgnoreCase)
            || dataType.Equals("long raw", StringComparison.OrdinalIgnoreCase)
            || dataType.Equals("blob", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeDataType(string dataType)
    {
        var normalized = dataType.Trim();
        var parenthesisIndex = normalized.IndexOf('(');

        return parenthesisIndex < 0
            ? normalized
            : normalized[..parenthesisIndex].Trim();
    }
}
