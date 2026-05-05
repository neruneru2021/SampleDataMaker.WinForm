namespace SampleDataMaker.Domain.Entities;

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
