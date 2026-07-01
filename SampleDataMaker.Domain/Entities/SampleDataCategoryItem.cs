namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 画面で選択できるカテゴリ名と項目名の組み合わせを表します。
/// </summary>
public class SampleDataCategoryItem
{
    public string CategoryName { get; }

    public string ItemName { get; }

    public string DisplayName => $"[{CategoryName}.{ItemName}]";

    public SampleDataCategoryItem(string categoryName, string itemName)
    {
        CategoryName = categoryName;
        ItemName = itemName;
    }
}
