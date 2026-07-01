namespace SampleDataMaker.Domain.Entities;

public class SampleDataFile
{
    public List<SampleDataItem> SingleItems { get; set; } = new();

    public List<SampleDataCategory> Categories { get; set; } = new();
}
