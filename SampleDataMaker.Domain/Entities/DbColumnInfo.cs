namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// DBカラムの名前、型、桁数、NULL許容などの定義情報を表します。
/// </summary>
public class DbColumnInfo
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public bool IsNullable { get; set; }

    public int OrdinalPosition { get; set; }

    public int? MaxLength { get; set; }

    public byte? NumericPrecision { get; set; }

    public int? NumericScale { get; set; }

    public bool IsIndexed { get; set; }

    public bool IsUniqueIndex { get; set; }
}
