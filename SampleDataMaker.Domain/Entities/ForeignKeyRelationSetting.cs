namespace SampleDataMaker.Domain.Entities;

public class ForeignKeyRelationSetting
{
    public string SourceSchemaName { get; set; } = string.Empty;

    public string SourceTableName { get; set; } = string.Empty;

    public string SourceColumnName { get; set; } = string.Empty;

    public string ReferenceSchemaName { get; set; } = string.Empty;

    public string ReferenceTableName { get; set; } = string.Empty;

    public string ReferenceColumnName { get; set; } = string.Empty;
}
