using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

/// <summary>
/// 通常のサンプルテストデータを生成するサービスを表します。
/// </summary>
public interface ITestDataGenerator
{
    /// <summary>
    /// 指定されたテーブルとカラム定義から、通常のサンプルテストデータを生成します。
    /// </summary>
    /// <param name="table">生成対象のテーブル情報。</param>
    /// <param name="columns">生成対象のカラム一覧。</param>
    /// <param name="sampleDataSettings">カラムごとに選択されたサンプルデータ設定。</param>
    /// <param name="rowCount">生成する行数。</param>
    /// <param name="columnStartNumbers">既存データに続けて採番するための、カラムごとの開始番号。</param>
    /// <returns>生成された通常テストデータ。</returns>
    GeneratedTestData Generate(
        DbTableInfo table,
        IReadOnlyList<DbColumnInfo> columns,
        IReadOnlyList<ColumnSampleDataSetting>? sampleDataSettings = null,
        int rowCount = 1,
        IReadOnlyDictionary<string, int>? columnStartNumbers = null);
}
