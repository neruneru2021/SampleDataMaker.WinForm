using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

/// <summary>
/// 既存テーブルに入っているキー列の値を調べ、追加入力時の採番開始位置を提供します。
/// </summary>
public interface IExistingKeyValueRepository
{
    /// <summary>
    /// 指定テーブルのユニークキー系カラムについて、DB上の現在の最大値を取得します。
    /// </summary>
    /// <param name="connectionInfo">接続先DB情報。</param>
    /// <param name="table">確認対象テーブル。</param>
    /// <param name="columns">確認対象テーブルのカラム定義。</param>
    /// <param name="cancellationToken">キャンセル要求を伝えるトークン。</param>
    /// <returns>カラム名をキー、既存データの最大値を値にした辞書。</returns>
    Task<IReadOnlyDictionary<string, int>> GetMaxValuesAsync(
        DbConnectionInfo connectionInfo,
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        CancellationToken cancellationToken = default);
}
