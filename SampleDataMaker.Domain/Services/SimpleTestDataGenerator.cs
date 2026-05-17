using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

/// <summary>
/// 通常のサンプルテストデータを生成します。
/// </summary>
public class SimpleTestDataGenerator : ITestDataGenerator
{
    private readonly ITestValueFactory _testValueFactory;
    private readonly ISampleDataRepository _sampleDataRepository;

    /// <summary>
    /// サンプルデータリポジトリを使って、通常テストデータ生成サービスを初期化します。
    /// </summary>
    /// <param name="sampleDataRepository">選択済みサンプルデータを取得するリポジトリ。</param>
    public SimpleTestDataGenerator(ISampleDataRepository sampleDataRepository)
        : this(new SimpleTestValueFactory(), sampleDataRepository)
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
    {
        _testValueFactory = testValueFactory;
        _sampleDataRepository = sampleDataRepository;
    }

    /// <summary>
    /// 指定されたテーブルとカラム定義から、通常のサンプルテストデータを生成します。
    /// </summary>
    /// <param name="table">生成対象のテーブル情報。</param>
    /// <param name="columns">生成対象のカラム一覧。</param>
    /// <param name="sampleDataSettings">カラムごとに選択されたサンプルデータ設定。</param>
    /// <param name="rowCount">生成する行数。</param>
    /// <returns>生成された通常テストデータ。</returns>
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
        _testValueFactory.StartGeneration();

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
    void StartGeneration();

    string Create(DbColumnInfo column);
}

internal class SimpleTestValueFactory : ITestValueFactory
{
    private readonly Func<DateTimeOffset> _now;
    private int _textValueNumber;
    private int _integerValueNumber;
    private int _decimalValueNumber;
    private int _binaryValueNumber;
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

    public void StartGeneration()
    {
        _textValueNumber = 0;
        _integerValueNumber = 0;
        _decimalValueNumber = 0;
        _binaryValueNumber = 0;
        _dateValue = _now().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string Create(DbColumnInfo column)
    {
        var dataType = NormalizeDataType(column.DataType);

        if (TextTypes.Contains(dataType))
        {
            return CreateText(column, dataType, ++_textValueNumber);
        }

        if (NumberTypes.Contains(dataType))
        {
            return DecimalNumberTypes.Contains(dataType)
                ? CreateDecimal(column, ++_decimalValueNumber)
                : (++_integerValueNumber).ToString();
        }

        if (DateTypes.Contains(dataType))
        {
            return _dateValue;
        }

        if (IsBinary(dataType))
        {
            return $"0x{++_binaryValueNumber:X2}";
        }

        return string.Empty;
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
        var maxLength = TryParseLength(column.DataType) ?? column.MaxLength;

        if (maxLength is null || maxLength <= 0)
        {
            return true;
        }

        if (dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase)
            && TryParseLength(column.DataType) is null)
        {
            maxLength = Math.Max(1, maxLength.Value / 2);
        }

        return value.Length <= maxLength;
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
