namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// テーブル単位で保存するカラム別サンプルデータ設定のテンプレートを表します。
/// </summary>
public class ColumnSampleDataTemplate
{
    public string TemplateName { get; set; } = string.Empty;

    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public List<ColumnSampleDataSetting> Columns { get; set; } = new();
}
