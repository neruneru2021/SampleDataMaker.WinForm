namespace SampleDataMaker.Domain.Entities;

public class ColumnSampleDataSetting
{
    public string ColumnName { get; set; } = string.Empty;

    public bool UseSampleData { get; set; }

    public string SampleDataKind { get; set; } = string.Empty;
}
