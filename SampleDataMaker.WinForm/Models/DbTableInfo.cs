namespace SampleDataMaker.WinForm.Models;

/// <summary>
/// WinForms側で表示するテーブル情報を表します。
/// </summary>
public class DbTableInfo
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DisplayName => $"{SchemaName}.{TableName}";
}
