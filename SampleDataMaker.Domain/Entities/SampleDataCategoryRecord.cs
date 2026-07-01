namespace SampleDataMaker.Domain.Entities;

public class SampleDataCategoryRecord
{
    public string Id { get; set; } = string.Empty;

    public Dictionary<string, string> Values { get; set; } = new();
}
