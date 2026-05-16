using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// テーブル一覧で選択状態や有効状態を持たせるための表示用アイテムです。
/// </summary>
internal class DbTableSelectionItem
{
    public bool IsSelected { get; set; }

    public DbTableInfo Table { get; }

    public string SchemaName => Table.SchemaName;

    public string TableName => Table.TableName;

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 表示対象のDBテーブル情報を保持します。
    /// </summary>
    public DbTableSelectionItem(DbTableInfo table)
    {
        Table = table;
    }
}
