using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.Services;

/// <summary>
/// 接続情報をもとにテーブル操作画面を開くためのナビゲーションを表します。
/// </summary>
public interface IConnectionOperationNavigator
{
    /// <summary>
    /// 指定されたDB接続のテーブル操作画面を表示します。
    /// </summary>
    Task Open(DbConnectionInfo connection);
}
