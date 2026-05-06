namespace SampleDataMaker.Domain.Entities;

public class ColumnSampleDataTemplate
{
    public string TemplateName { get; set; } = string.Empty;

    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public List<ColumnSampleDataSetting> Columns { get; set; } = new();
}
