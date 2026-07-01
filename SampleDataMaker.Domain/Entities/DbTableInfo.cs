namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// DBテーブルを識別するスキーマ名とテーブル名を表します。
/// </summary>
public class DbTableInfo
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DisplayName => $"{SchemaName}.{TableName}";
}
