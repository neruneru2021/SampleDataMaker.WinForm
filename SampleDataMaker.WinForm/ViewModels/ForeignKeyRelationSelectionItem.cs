using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

internal class ForeignKeyRelationSelectionItem
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public ForeignKeyRelationSetting ToSetting(DbColumnInfo sourceColumn)
    {
        return new ForeignKeyRelationSetting
        {
            SourceSchemaName = sourceColumn.SchemaName,
            SourceTableName = sourceColumn.TableName,
            SourceColumnName = sourceColumn.ColumnName,
            ReferenceSchemaName = SchemaName,
            ReferenceTableName = TableName,
            ReferenceColumnName = ColumnName
        };
    }
}
