namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 通常データとカテゴリデータをまとめたサンプルデータファイルを表します。
/// </summary>
public class SampleDataFile
{
    public List<SampleDataItem> SingleItems { get; set; } = new();

    public List<SampleDataCategory> Categories { get; set; } = new();
}
