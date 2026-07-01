using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

/// <summary>
/// 通常のサンプルテストデータを生成します。
/// </summary>
public class SimpleTestDataGenerator : ITestDataGenerator
{
    private readonly ITestValueFactory _testValueFactory;
    private readonly ITestValueFactory _randomTestValueFactory;
    private readonly ISampleDataRepository _sampleDataRepository;
    private readonly Random _categoryRandom;

    /// <summary>
    /// サンプルデータリポジトリを使って、通常テストデータ生成サービスを初期化します。
    /// </summary>
    /// <param name="sampleDataRepository">選択済みサンプルデータを取得するリポジトリ。</param>
    public SimpleTestDataGenerator(ISampleDataRepository sampleDataRepository)
        : this(
            new SimpleTestValueFactory(),
            new RandomTestValueFactory(),
            sampleDataRepository,
            new Random())
    {
    }

    /// <summary>
    /// テスト値ファクトリを差し替えて、通常テストデータ生成サービスを初期化します。
    /// </summary>
    /// <param name="testValueFactory">型ごとの通常テスト値を作成するファクトリ。</param>
    /// <param name="sampleDataRepository">選択済みサンプルデータを取得するリポジトリ。</param>
    internal SimpleTestDataGenerator(
        ITestValueFactory testValueFactory,
        ISampleDataRepository sampleDataRepository)
        : this(
            testValueFactory,
            new RandomTestValueFactory(),
            sampleDataRepository,
            new Random())
    {
    }

    internal SimpleTestDataGenerator(
        ITestValueFactory testValueFactory,
        ITestValueFactory randomTestValueFactory,
        ISampleDataRepository sampleDataRepository)
        : this(
            testValueFactory,
            randomTestValueFactory,
            sampleDataRepository,
            new Random())
    {
    }

    internal SimpleTestDataGenerator(
        ITestValueFactory testValueFactory,
        ITestValueFactory randomTestValueFactory,
        ISampleDataRepository sampleDataRepository,
        Random categoryRandom)
    {
        _testValueFactory = testValueFactory;
        _randomTestValueFactory = randomTestValueFactory;
        _sampleDataRepository = sampleDataRepository;
        _categoryRandom = categoryRandom;
    }

    /// <summary>
    /// 指定されたテーブルとカラム定義から、通常のサンプルテストデータを生成します。
    /// </summary>
    /// <param name="table">生成対象のテーブル情報。</param>
    /// <param name="columns">生成対象のカラム一覧。</param>
    /// <param name="sampleDataSettings">カラムごとに選択されたサンプルデータ設定。</param>
    /// <param name="rowCount">生成する行数。</param>
    /// <param name="columnStartNumbers">既存データに続けて採番するための、カラムごとの開始番号。</param>
    /// <returns>生成された通常テストデータ。</returns>
    public GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<ColumnSampleDataSetting>? sampleDataSettings = null,
        int rowCount = 1,
        IReadOnlyDictionary<string, int>? columnStartNumbers = null)
    {
        var sampleProvider = new SampleDataValueProvider(
            sampleDataSettings ?? Array.Empty<ColumnSampleDataSetting>(),
            _sampleDataRepository,
            _categoryRandom);
        var sampleDataSettingsByColumn = (sampleDataSettings ?? Array.Empty<ColumnSampleDataSetting>())
            .GroupBy(setting => setting.ColumnName)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var orderedColumns = columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();
        var rows = new List<IReadOnlyDictionary<string, string?>>();
        var rowMetadata = new List<GeneratedRowMetadata>();
        _testValueFactory.StartGeneration(columnStartNumbers);
        _randomTestValueFactory.StartGeneration(columnStartNumbers);

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var metadataByColumn = new Dictionary<string, GeneratedColumnMetadata>(
                StringComparer.OrdinalIgnoreCase);
            var selectedCategoryRecords = new Dictionary<string, SampleDataCategoryRecord>(
                StringComparer.Ordinal);

            foreach (var column in orderedColumns)
            {
                if (sampleDataSettingsByColumn.TryGetValue(column.ColumnName, out var setting)
                    && SampleDataKindNames.IsRandom(setting.SampleDataKind))
                {
                    row[column.ColumnName] = _randomTestValueFactory.Create(column);
                    continue;
                }

                if (sampleProvider.TryCreate(
                    column,
                    rowIndex,
                    selectedCategoryRecords,
                    out var sampleValue,
                    out var metadata))
                {
                    row[column.ColumnName] = sampleValue;

                    if (metadata != null)
                    {
                        metadataByColumn[column.ColumnName] = metadata;
                    }

                    continue;
                }

                row[column.ColumnName] = _testValueFactory.Create(column);
            }

            rows.Add(row);
            rowMetadata.Add(new GeneratedRowMetadata(rowIndex, metadataByColumn));
        }

        return new GeneratedTestData(
            table,
            orderedColumns,
            rows,
            rowMetadata);
    }
}

internal interface ITestValueFactory
{
    void StartGeneration(IReadOnlyDictionary<string, int>? columnStartNumbers = null);

    string? Create(DbColumnInfo column);
}

internal class SimpleTestValueFactory : ITestValueFactory
{
    private readonly Func<DateTimeOffset> _now;
    private readonly Dictionary<string, int> _textValueNumbers = new();
    private readonly Dictionary<string, int> _integerValueNumbers = new();
    private readonly Dictionary<string, int> _decimalValueNumbers = new();
    private readonly Dictionary<string, int> _binaryValueNumbers = new();
    private readonly Dictionary<string, int> _columnStartNumbers = new(StringComparer.OrdinalIgnoreCase);
    private string _dateValue = string.Empty;

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

    private static readonly HashSet<string> DecimalNumberTypes = new(StringComparer.OrdinalIgnoreCase)
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

    private static readonly HashSet<string> AdjustableTextTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "nchar",
        "nvarchar",
        "nvarchar2",
        "ntext",
        "nclob"
    };

    public SimpleTestValueFactory()
        : this(() => TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")))
    {
    }

    internal SimpleTestValueFactory(Func<DateTimeOffset> now)
    {
        _now = now;
    }

    public void StartGeneration(IReadOnlyDictionary<string, int>? columnStartNumbers = null)
    {
        _textValueNumbers.Clear();
        _integerValueNumbers.Clear();
        _decimalValueNumbers.Clear();
        _binaryValueNumbers.Clear();
        _columnStartNumbers.Clear();
        _dateValue = _now().ToString("yyyy-MM-dd HH:mm:ss");

        if (columnStartNumbers == null)
        {
            return;
        }

        foreach (var pair in columnStartNumbers)
        {
            _columnStartNumbers[pair.Key] = pair.Value;
        }
    }

    public string Create(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        if (TextTypes.Contains(dataType))
        {
            return CreateText(column, dataType, NextValue(_textValueNumbers, column, GetTextMaximum(column, dataType)));
        }

        if (NumberTypes.Contains(dataType))
        {
            return DecimalNumberTypes.Contains(dataType)
                ? CreateDecimal(column, NextValue(_decimalValueNumbers, column, GetDecimalIntegerPartMaximum(column)))
                : NextValue(_integerValueNumbers, column, GetIntegerMaximum(dataType)).ToString();
        }

        if (DateTypes.Contains(dataType))
        {
            return _dateValue;
        }

        if (IsBinary(dataType))
        {
            return $"0x{NextValue(_binaryValueNumbers, column, GetBinaryMaximum(column, dataType)):X2}";
        }

        return string.Empty;
    }

    private int NextValue(
        Dictionary<string, int> valueNumbers,
        DbColumnInfo column,
        int maximum)
    {
        var key = CreateColumnKey(column);
        var hasStartNumber = _columnStartNumbers.TryGetValue(key, out var startNumber);
        var current = valueNumbers.TryGetValue(key, out var value)
            ? value
            : hasStartNumber
                ? startNumber
                : 0;
        var next = current + 1;

        if (next > maximum)
        {
            if (hasStartNumber)
            {
                throw new InvalidOperationException(
                    $"{column.SchemaName}.{column.TableName}.{column.ColumnName} は既存データの最大値 {current} から {maximum} を超えるため、キー重複を避けて追加作成できません。");
            }

            next = 1;
        }

        valueNumbers[key] = next;

        return next;
    }

    private static string CreateText(DbColumnInfo column, string dataType, int valueNumber)
    {
        var textKind = AdjustableTextTypes.Contains(dataType)
            ? "Adjustable"
            : "Fixed";
        var lengthText = CreateLengthText(column, dataType);
        var typeText = lengthText is null
            ? dataType.ToUpperInvariant()
            : $"{dataType.ToUpperInvariant()}({lengthText})";
        var candidates = textKind == "Adjustable"
            ? new[]
            {
                $"{valueNumber}-{textKind}-{typeText}",
                lengthText is null ? $"{valueNumber}-{textKind}" : $"{valueNumber}-{textKind} {lengthText}",
                $"{valueNumber}-{textKind}",
                valueNumber.ToString()
            }
            : new[]
            {
                $"{valueNumber}-{textKind}-{typeText}",
                lengthText is null ? $"{valueNumber}-{textKind}" : $"{valueNumber}-{textKind}({lengthText})",
                $"{valueNumber}-{textKind}",
                valueNumber.ToString()
            };

        return candidates.FirstOrDefault(candidate => FitsTextLength(candidate, column, dataType))
            ?? valueNumber.ToString();
    }

    private static string CreateDecimal(DbColumnInfo column, int valueNumber)
    {
        var scale = Math.Max(1, column.NumericScale.GetValueOrDefault(3));

        return $"{valueNumber}.{new string('0', scale - 1)}1";
    }

    private static int GetIntegerMaximum(string dataType)
    {
        return dataType.ToLowerInvariant() switch
        {
            "bit" => 1,
            "tinyint" => byte.MaxValue,
            "smallint" => short.MaxValue,
            _ => int.MaxValue
        };
    }

    private static int GetDecimalIntegerPartMaximum(DbColumnInfo column)
    {
        var precision = column.NumericPrecision;
        if (precision == null)
        {
            return int.MaxValue;
        }

        var scale = column.NumericScale.GetValueOrDefault(0);
        var integerDigits = Math.Max(1, precision.Value - scale);

        return CreateMaximumByDigits(integerDigits);
    }

    private static int GetTextMaximum(DbColumnInfo column, string dataType)
    {
        var maxLength = GetEffectiveTextLength(column, dataType);

        return maxLength == null || maxLength <= 0
            ? int.MaxValue
            : CreateMaximumByDigits(maxLength.Value);
    }

    private static int GetBinaryMaximum(DbColumnInfo column, string dataType)
    {
        var length = TryParseLength(column.DataType) ?? column.MaxLength;

        if (length == null || length <= 0)
        {
            return int.MaxValue;
        }

        return CreateMaximumByDigits(length.Value * 2);
    }

    private static int CreateMaximumByDigits(int digits)
    {
        if (digits <= 0)
        {
            return 1;
        }

        if (digits >= 10)
        {
            return int.MaxValue;
        }

        return (int)Math.Pow(10, digits) - 1;
    }

    private static string? CreateLengthText(DbColumnInfo column, string dataType)
    {
        var parsedLength = TryParseLength(column.DataType);
        var length = parsedLength ?? column.MaxLength;

        if (length is null)
        {
            return null;
        }

        if (length <= 0)
        {
            return "MAX";
        }

        if (dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase)
            && parsedLength is null)
        {
            length = Math.Max(1, length.Value / 2);
        }

        return length.Value.ToString();
    }

    private static bool FitsTextLength(string value, DbColumnInfo column, string dataType)
    {
        var maxLength = GetEffectiveTextLength(column, dataType);

        if (maxLength is null || maxLength <= 0)
        {
            return true;
        }

        return value.Length <= maxLength;
    }

    private static int? GetEffectiveTextLength(DbColumnInfo column, string dataType)
    {
        var parsedLength = TryParseLength(column.DataType);
        var maxLength = parsedLength ?? column.MaxLength;

        if (maxLength is null || maxLength <= 0)
        {
            return maxLength;
        }

        if (dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase)
            && parsedLength is null)
        {
            maxLength = Math.Max(1, maxLength.Value / 2);
        }

        return maxLength;
    }

    private static int? TryParseLength(string dataType)
    {
        var startIndex = dataType.IndexOf('(');
        var endIndex = dataType.IndexOf(')');

        if (startIndex < 0 || endIndex <= startIndex)
        {
            return null;
        }

        var lengthText = dataType[(startIndex + 1)..endIndex].Trim();

        return int.TryParse(lengthText, out var length)
            ? length
            : null;
    }

    private static bool IsBinary(string dataType)
    {
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

    private static string CreateColumnKey(DbColumnInfo column)
    {
        return $"{column.SchemaName}.{column.TableName}.{column.ColumnName}";
    }
}

internal class RandomTestValueFactory : ITestValueFactory
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private readonly Random _random;

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
        "tinyint",
        "bit"
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

    public RandomTestValueFactory()
        : this(new Random())
    {
    }

    internal RandomTestValueFactory(Random random)
    {
        _random = random;
    }

    public void StartGeneration(IReadOnlyDictionary<string, int>? columnStartNumbers = null)
    {
    }

    public string? Create(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        if (column.IsNullable && _random.Next(0, 10) == 0)
        {
            return null;
        }

        if (TextTypes.Contains(dataType))
        {
            return CreateText(column, dataType);
        }

        if (IntegerTypes.Contains(dataType))
        {
            return CreateInteger(dataType);
        }

        if (DecimalTypes.Contains(dataType))
        {
            return CreateDecimal(column);
        }

        if (DateTypes.Contains(dataType))
        {
            return CreateDate(dataType);
        }

        if (IsBinary(dataType))
        {
            return $"0x{_random.Next(0, 256):X2}";
        }

        return string.Empty;
    }

    private string CreateText(DbColumnInfo column, string dataType)
    {
        if (_random.Next(0, 10) == 0 && CanUseEmptyString(dataType))
        {
            return string.Empty;
        }

        var maxLength = GetEffectiveTextLength(column, dataType);
        if (maxLength == null || maxLength <= 0)
        {
            maxLength = 10;
        }

        var length = _random.Next(1, Math.Min(maxLength.Value, 32) + 1);

        return new string(
            Enumerable
                .Range(0, length)
                .Select(_ => Characters[_random.Next(Characters.Length)])
                .ToArray());
    }

    private string CreateInteger(string dataType)
    {
        return dataType.ToLowerInvariant() switch
        {
            "bit" => _random.Next(0, 2).ToString(),
            "tinyint" => _random.Next(byte.MinValue, byte.MaxValue + 1).ToString(),
            "smallint" => _random.Next(short.MinValue, short.MaxValue + 1).ToString(),
            _ => _random.Next(0, int.MaxValue).ToString()
        };
    }

    private string CreateDecimal(DbColumnInfo column)
    {
        var scale = Math.Max(0, column.NumericScale.GetValueOrDefault(2));
        var integerDigits = Math.Max(1, column.NumericPrecision.GetValueOrDefault((byte)9) - scale);
        var integerMaximum = CreateMaximumByDigits(integerDigits);
        var integerPart = _random.Next(0, integerMaximum + 1).ToString();

        if (scale == 0)
        {
            return integerPart;
        }

        var decimalPart = _random.Next(0, CreateMaximumByDigits(scale) + 1)
            .ToString()
            .PadLeft(scale, '0');

        return $"{integerPart}.{decimalPart}";
    }

    private string CreateDate(string dataType)
    {
        var date = new DateTime(2026, 1, 1)
            .AddDays(_random.Next(0, 365))
            .AddSeconds(_random.Next(0, 24 * 60 * 60));

        return dataType.Equals("time", StringComparison.OrdinalIgnoreCase)
            ? date.ToString("HH:mm:ss")
            : dataType.Equals("date", StringComparison.OrdinalIgnoreCase)
                ? date.ToString("yyyy-MM-dd")
            : date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private static int CreateMaximumByDigits(int digits)
    {
        if (digits <= 0)
        {
            return 1;
        }

        if (digits >= 9)
        {
            return int.MaxValue - 1;
        }

        return (int)Math.Pow(10, digits) - 1;
    }

    private static bool CanUseEmptyString(string dataType)
    {
        return !dataType.Equals("uniqueidentifier", StringComparison.OrdinalIgnoreCase)
            && !dataType.Equals("xml", StringComparison.OrdinalIgnoreCase);
    }

    private static int? GetEffectiveTextLength(DbColumnInfo column, string dataType)
    {
        var parsedLength = TryParseLength(column.DataType);
        var maxLength = parsedLength ?? column.MaxLength;

        if (maxLength is null || maxLength <= 0)
        {
            return maxLength;
        }

        if (dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase)
            && parsedLength is null)
        {
            maxLength = Math.Max(1, maxLength.Value / 2);
        }

        return maxLength;
    }

    private static int? TryParseLength(string dataType)
    {
        var startIndex = dataType.IndexOf('(');
        var endIndex = dataType.IndexOf(')');

        if (startIndex < 0 || endIndex <= startIndex)
        {
            return null;
        }

        var lengthText = dataType[(startIndex + 1)..endIndex].Trim();

        return int.TryParse(lengthText, out var length)
            ? length
            : null;
    }

    private static bool IsBinary(string dataType)
    {
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
