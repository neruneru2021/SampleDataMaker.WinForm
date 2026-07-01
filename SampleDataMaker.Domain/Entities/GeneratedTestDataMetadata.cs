namespace SampleDataMaker.Domain.Entities;

public class GeneratedRowMetadata
{
    public int RowIndex { get; }

    public IReadOnlyDictionary<string, GeneratedColumnMetadata> Columns { get; }

    public IReadOnlySet<string> BoundaryValueColumns { get; }

    public GeneratedRowMetadata(
        int rowIndex,
        IReadOnlyDictionary<string, GeneratedColumnMetadata>? columns = null,
        IReadOnlySet<string>? boundaryValueColumns = null)
    {
        RowIndex = rowIndex;
        Columns = columns ?? new Dictionary<string, GeneratedColumnMetadata>();
        BoundaryValueColumns = boundaryValueColumns ?? new HashSet<string>();
    }
}

public class GeneratedColumnMetadata
{
    public string ColumnName { get; }

    public string CategoryName { get; }

    public string CategoryItemName { get; }

    public string CategoryRecordId { get; }

    public bool IsForeignKeyInherited { get; }

    public GeneratedColumnMetadata(
        string columnName,
        string categoryName,
        string categoryItemName,
        string categoryRecordId,
        bool isForeignKeyInherited = false)
    {
        ColumnName = columnName;
        CategoryName = categoryName;
        CategoryItemName = categoryItemName;
        CategoryRecordId = categoryRecordId;
        IsForeignKeyInherited = isForeignKeyInherited;
    }

    public GeneratedColumnMetadata AsForeignKeyInherited(string columnName)
    {
        return new GeneratedColumnMetadata(
            columnName,
            CategoryName,
            CategoryItemName,
            CategoryRecordId,
            isForeignKeyInherited: true);
    }
}
