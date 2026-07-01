namespace SampleDataMaker.Domain.Entities;

/// <summary>
/// テストデータの出力先と、作成したファイルの一覧を表します。
/// </summary>
public class TestDataOutputResult
{
    public string OutputDirectoryPath { get; }

    public IReadOnlyList<string> FilePaths { get; }

    public TestDataOutputResult(
        string outputDirectoryPath,
        IReadOnlyList<string> filePaths)
    {
        OutputDirectoryPath = outputDirectoryPath;
        FilePaths = filePaths;
    }
}
