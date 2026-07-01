using ChainingAssertion;
using SampleDataMaker.Infrastructure.Json;

namespace SampleDataMaker.xUnit.InfrastructureTests;

/// <summary>
/// 外部キー設定JSONの方向情報を正しく移行できることを確認します。
/// </summary>
[TestClass]
public sealed class JsonForeignKeyRelationRepositoryTests
{
    [TestMethod]
    public void 旧形式の双方向設定は先頭を正方向として移行する()
    {
        // Arrange
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            $"SampleDataMaker-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, "foreign-key-relations.json");
        File.WriteAllText(
            filePath,
            """
            [
              {
                "sourceSchemaName": "dbo",
                "sourceTableName": "Users",
                "sourceColumnName": "ClinicId",
                "referenceSchemaName": "dbo",
                "referenceTableName": "Clinics",
                "referenceColumnName": "ClinicId"
              },
              {
                "sourceSchemaName": "dbo",
                "sourceTableName": "Clinics",
                "sourceColumnName": "ClinicId",
                "referenceSchemaName": "dbo",
                "referenceTableName": "Users",
                "referenceColumnName": "ClinicId"
              }
            ]
            """);

        try
        {
            // Act
            var settings = new JsonForeignKeyRelationRepository(filePath).GetAll();

            // Assert
            settings.Count.Is(2);
            settings[0].IsReverse.IsFalse();
            settings[1].IsReverse.IsTrue();
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
