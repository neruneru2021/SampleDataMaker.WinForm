namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// DBカラムに割り当てる通常種類またはカテゴリ項目の設定を表します。
/// </summary>
public class ColumnSampleDataSetting
{
    public string ColumnName { get; set; } = string.Empty;

    public string SampleDataKind { get; set; } = string.Empty;

    public string? CategoryName { get; set; }

    public string? CategoryItemName { get; set; }

    public bool IsCategory =>
        !string.IsNullOrWhiteSpace(CategoryName)
        && !string.IsNullOrWhiteSpace(CategoryItemName);
}
