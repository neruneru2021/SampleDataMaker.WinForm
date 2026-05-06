using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

internal class DbColumnSampleDataSelectionItem
{
    public DbColumnInfo Column { get; }

    public string ColumnName => Column.ColumnName;

    public string DataType => Column.DataType;

    public bool UseSampleData { get; set; }

    public string SampleDataKind { get; set; } = string.Empty;

    public DbColumnSampleDataSelectionItem(DbColumnInfo column)
    {
        Column = column;
    }

    public ColumnSampleDataSetting ToSetting()
    {
        return new ColumnSampleDataSetting
        {
            ColumnName = ColumnName,
            UseSampleData = UseSampleData,
            SampleDataKind = SampleDataKind
        };
    }
}
