using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.Services;

/// <summary>
/// 外部キー設定時のデータ型不一致をMessageBoxで利用者に確認します。
/// </summary>
internal class ForeignKeyTypeMismatchConfirmationService : IForeignKeyTypeMismatchConfirmationService
{
    /// <summary>
    /// 型が異なる外部キー設定を続行するか確認します。
    /// </summary>
    public bool Confirm(DbColumnInfo sourceColumn, DbColumnInfo referenceColumn)
    {
        var message = $"""
            外部キー設定元と参照先のデータ型が異なります。

            設定元:
            {sourceColumn.SchemaName}.{sourceColumn.TableName}.{sourceColumn.ColumnName}
            DataType: {sourceColumn.DataType}

            参照先:
            {referenceColumn.SchemaName}.{referenceColumn.TableName}.{referenceColumn.ColumnName}
            DataType: {referenceColumn.DataType}

            このまま外部キーとして設定しますか？
            """;

        return MessageBox.Show(
            message,
            "データ型不一致の確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }
}
