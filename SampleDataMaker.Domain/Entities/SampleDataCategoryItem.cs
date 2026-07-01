namespace SampleDataMaker.Domain.Entities;

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
