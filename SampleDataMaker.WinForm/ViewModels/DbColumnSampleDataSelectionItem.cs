using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// カラム一覧でサンプルデータ利用設定を編集するための表示用アイテムです。
/// </summary>
internal class DbColumnSampleDataSelectionItem
{
    public DbColumnInfo Column { get; }

    public string ColumnName => Column.ColumnName;

    public string DataType => Column.DataType;

    public string SampleDataKind { get; set; } = string.Empty;

    public string ForeignKeyDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 表示対象のDBカラム情報を保持します。
    /// </summary>
    public DbColumnSampleDataSelectionItem(DbColumnInfo column)
    {
        Column = column;
    }

    /// <summary>
    /// 画面で編集したサンプルデータ設定を保存用エンティティに変換します。
    /// </summary>
    public ColumnSampleDataSetting ToSetting()
    {
        return new ColumnSampleDataSetting
        {
            ColumnName = ColumnName,
            SampleDataKind = SampleDataKind
        };
    }
}
