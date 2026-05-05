namespace SampleDataMaker.Domain.Entities;

public class GeneratedTestData
{
    public DbTableInfo Table { get; }

    public IReadOnlyList<DbColumnInfo> Columns { get; }

    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows { get; }

    public GeneratedTestData(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows)
    {
        Table = table;
        Columns = columns;
        Rows = rows;
    }
}
