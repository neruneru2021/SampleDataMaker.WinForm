namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// 1テーブル分の生成値と、その値を追跡するメタ情報を保持します。
/// </summary>
public class GeneratedTestData
{
    public DbTableInfo Table { get; }

    public IReadOnlyList<DbColumnInfo> Columns { get; }

    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows { get; }

    public IReadOnlyList<GeneratedRowMetadata> RowMetadata { get; }

    public GeneratedTestData(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        IReadOnlyList<GeneratedRowMetadata>? rowMetadata = null)
    {
        Table = table;
        Columns = columns;
        Rows = rows;
        RowMetadata = rowMetadata
            ?? rows
                .Select((_, index) => new GeneratedRowMetadata(index))
                .ToList();

        if (RowMetadata.Count != Rows.Count)
        {
            throw new ArgumentException(
                "生成行と行メタ情報の件数は一致している必要があります。",
                nameof(rowMetadata));
        }
    }
}
