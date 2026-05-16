using ChainingAssertion;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Infrastructure.Json;

namespace SampleDataMaker.xUnit.InfrastructureTests;

/// <summary>
/// DB接続情報をJSONファイルへ保存・読込するリポジトリを確認します。
///
/// Infrastructure層のテストでは、外部DBではなく一時フォルダ内のファイルだけを使います。
/// これにより、テスト実行環境に依存しにくい単体テストになります。
/// </summary>
[TestClass]
public sealed class JsonDbConnectionInfoRepositoryTests
{
    private string _tempDirectoryPath = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _tempDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "SampleDataMakerTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_tempDirectoryPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDirectoryPath))
        {
            Directory.Delete(_tempDirectoryPath, recursive: true);
        }
    }

    [TestMethod]
    public void ファイルが存在しない場合は空の一覧を返す()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectoryPath, "connections.json");
        var repository = new JsonDbConnectionInfoRepository(filePath);

        // Act
        var result = repository.GetAll();

        // Assert
        result.Count.Is(0);
    }

    [TestMethod]
    public void SaveAllで保存した接続情報をGetAllで復元できる()
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectoryPath, "connections.json");
        var repository = new JsonDbConnectionInfoRepository(filePath);
        var connections = new[]
        {
            new DbConnectionInfo
            {
                Title = "Oracle PLM",
                DbType = DbTypeKind.Oracle,
                ConnectionString = "User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;",
                DefaultSchema = "PLMCONSOLE"
            }
        };

        // Act
        repository.SaveAll(connections);
        var result = repository.GetAll();

        // Assert
        result.Count.Is(1);
        result[0].Title.Is("Oracle PLM");
        result[0].DbType.Is(DbTypeKind.Oracle);
        result[0].ConnectionString.Is("User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;");
        result[0].DefaultSchema.Is("PLMCONSOLE");
    }
}
