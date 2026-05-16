using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// 外部キー候補として選択された参照先カラムを表示するためのアイテムです。
/// </summary>
internal class ForeignKeyRelationSelectionItem
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// 選択された参照先と設定元カラムを外部キー設定に変換します。
    /// </summary>
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
