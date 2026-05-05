using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

internal class DbTableSelectionItem
{
    public bool IsSelected { get; set; }

    public DbTableInfo Table { get; }

    public string SchemaName => Table.SchemaName;

    public string TableName => Table.TableName;

    public DbTableSelectionItem(DbTableInfo table)
    {
        Table = table;
    }
}
