namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 同じ対象に属するカテゴリ項目の値を1レコードとして保持します。
/// </summary>
public class SampleDataCategoryRecord
{
    public string Id { get; set; } = string.Empty;

    public Dictionary<string, string> Values { get; set; } = new();
}
