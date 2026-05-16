using ChainingAssertion;
using Moq;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;

namespace SampleDataMaker.xUnit.DomainTests;

/// <summary>
/// 通常テストデータ生成の基本的な振る舞いを確認します。
///
/// このテストでは、外部ファイルやDBには触れません。
/// ISampleDataRepositoryをMoqで差し替えることで、
/// 「サンプル種類が選択された時だけマスタ値を使う」という仕様だけに集中します。
/// </summary>
[TestClass]
public sealed class SimpleTestDataGeneratorTests
{
    [TestMethod]
    public void 種類が選択されたカラムはサンプルデータから値を作成する()
    {
        // Arrange
        // テスト対象が必要とするサンプルデータリポジトリをMoqで作ります。
        // GetValues("都道府県") が呼ばれたら、固定の2件を返すようにしています。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        sampleDataRepositoryMock
            .Setup(x => x.GetValues("都道府県"))
            .Returns(new[] { "東京", "大阪" });

        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Users");
        var columns = new[]
        {
            CreateColumn("Name", "varchar", ordinalPosition: 1)
        };
        var settings = new[]
        {
            new ColumnSampleDataSetting
            {
                ColumnName = "Name",
                SampleDataKind = "都道府県"
            }
        };

        // Act
        // 3行作成すると、サンプルデータ2件が循環して使われます。
        var result = generator.Generate(table, columns, settings, rowCount: 3);

        // Assert
        result.Rows.Count.Is(3);
        result.Rows[0]["Name"].Is("東京");
        result.Rows[1]["Name"].Is("大阪");
        result.Rows[2]["Name"].Is("東京");
    }

    [TestMethod]
    public void 種類が未選択のカラムはデータ型に応じたデフォルト値を作成する()
    {
        // Arrange
        // 種類未選択の時はサンプルデータリポジトリを使わないため、
        // MockにはSetupを書かず「呼ばれないこと」をVerifyで確認します。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();

        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Users");
        var columns = new[]
        {
            CreateColumn("Id", "int", ordinalPosition: 1),
            CreateColumn("Name", "varchar", ordinalPosition: 2),
            CreateColumn("CreatedAt", "datetime", ordinalPosition: 3)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 1);

        // Assert
        result.Rows[0]["Id"].Is("1");
        result.Rows[0]["Name"].Is("A");
        result.Rows[0]["CreatedAt"].Is("2026-01-01");

        sampleDataRepositoryMock.Verify(x => x.GetValues(It.IsAny<string>()), Times.Never);
    }

    private static DbTableInfo CreateTable(string schemaName, string tableName)
    {
        return new DbTableInfo
        {
            SchemaName = schemaName,
            TableName = tableName
        };
    }

    private static DbColumnInfo CreateColumn(
        string columnName,
        string dataType,
        int ordinalPosition)
    {
        return new DbColumnInfo
        {
            SchemaName = "dbo",
            TableName = "Users",
            ColumnName = columnName,
            DataType = dataType,
            OrdinalPosition = ordinalPosition
        };
    }
}
