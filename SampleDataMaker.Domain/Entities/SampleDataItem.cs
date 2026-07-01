namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 従来形式の種類名と単独のサンプル値を表します。
/// </summary>
public class SampleDataItem
{
    public string Kind { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
