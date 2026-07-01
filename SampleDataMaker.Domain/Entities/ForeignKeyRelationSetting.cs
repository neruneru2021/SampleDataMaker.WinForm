namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 外部キーで結ばれた参照元カラムと参照先カラムの関係を表します。
/// </summary>
public class ForeignKeyRelationSetting
{
    public string SourceSchemaName { get; set; } = string.Empty;

    public string SourceTableName { get; set; } = string.Empty;

    public string SourceColumnName { get; set; } = string.Empty;

    public string ReferenceSchemaName { get; set; } = string.Empty;

    public string ReferenceTableName { get; set; } = string.Empty;

    public string ReferenceColumnName { get; set; } = string.Empty;

    public bool IsReverse { get; set; }
}
