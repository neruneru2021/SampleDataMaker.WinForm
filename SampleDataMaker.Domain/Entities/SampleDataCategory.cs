namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 同じ意味を持つ複数項目をまとめたサンプルデータカテゴリを表します。
/// </summary>
public class SampleDataCategory
{
    public string Name { get; set; } = string.Empty;

    public List<SampleDataCategoryRecord> Records { get; set; } = new();
}
