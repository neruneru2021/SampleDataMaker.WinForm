namespace SampleDataMaker.Domain.Entities;

public class ColumnSampleDataSetting
{
    public string ColumnName { get; set; } = string.Empty;

    public string SampleDataKind { get; set; } = string.Empty;

    public string? CategoryName { get; set; }

    public string? CategoryItemName { get; set; }

    public bool IsCategory =>
        !string.IsNullOrWhiteSpace(CategoryName)
        && !string.IsNullOrWhiteSpace(CategoryItemName);
}
