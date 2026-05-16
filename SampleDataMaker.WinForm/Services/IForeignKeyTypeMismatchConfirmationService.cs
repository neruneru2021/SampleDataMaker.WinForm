using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.Services;

/// <summary>
/// 外部キー設定元と参照先のデータ型が異なる場合に、追加してよいか確認します。
/// </summary>
internal interface IForeignKeyTypeMismatchConfirmationService
{
    /// <summary>
    /// 型が異なる外部キー設定を続行する場合はtrue、キャンセルする場合はfalseを返します。
    /// </summary>
    bool Confirm(DbColumnInfo sourceColumn, DbColumnInfo referenceColumn);
}
