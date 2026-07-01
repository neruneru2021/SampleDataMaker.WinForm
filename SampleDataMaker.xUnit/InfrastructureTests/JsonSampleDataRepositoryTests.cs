using ChainingAssertion;
using SampleDataMaker.Infrastructure.Json;

namespace SampleDataMaker.xUnit.InfrastructureTests;

/// <summary>
/// 新形式のサンプルデータJSONを正しく読み込めることを確認します。
/// </summary>
[TestClass]
public sealed class JsonSampleDataRepositoryTests
{
    [TestMethod]
    public void 新形式JSONから通常種類とカテゴリ項目を読み込む()
    {
        // Arrange
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            $"SampleDataMaker-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, "sample-data.json");
        File.WriteAllText(
            filePath,
            """
            {
              "singleItems": [
                {
                  "kind": "氏名",
                  "value": "小嶋 彩花"
                }
              ],
              "categories": [
                {
                  "name": "個人情報セット",
                  "records": [
                    {
                      "id": "person-001",
                      "values": {
                        "苗字": "加藤",
                        "苗字かな": "かとう"
                      }
                    }
                  ]
                }
              ]
            }
            """);

        try
        {
            // Act
            var repository = new JsonSampleDataRepository(filePath);
            var categoryItems = repository.GetCategoryItems();
            var found = repository.TryGetCategoryRecord(
                "個人情報セット",
                "person-001",
                out var record);

            // Assert
            repository.GetKinds().Single().Is("氏名");
            repository.GetValues("氏名").Single().Is("小嶋 彩花");
            categoryItems.Select(item => item.DisplayName).ToArray()
                .Is(new[]
                {
                    "[個人情報セット.苗字]",
                    "[個人情報セット.苗字かな]"
                });
            found.IsTrue();
            record!.Values["苗字かな"].Is("かとう");
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
