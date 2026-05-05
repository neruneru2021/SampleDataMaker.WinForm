namespace SampleDataMaker.Domain.Entities;

public class DbTableInfo
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DisplayName => $"{SchemaName}.{TableName}";
}
