namespace SampleDataMaker.Domain.Entities;

public class SampleDataCategory
{
    public string Name { get; set; } = string.Empty;

    public List<SampleDataCategoryRecord> Records { get; set; } = new();
}
