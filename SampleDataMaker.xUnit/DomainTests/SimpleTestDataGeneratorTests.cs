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

        var generator = new SimpleTestDataGenerator(
            new SimpleTestValueFactory(() => new DateTimeOffset(2026, 5, 17, 10, 30, 45, TimeSpan.FromHours(9))),
            sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Users");
        var columns = new[]
        {
            CreateColumn("Id", "int", ordinalPosition: 1),
            CreateColumn("Name", "varchar", ordinalPosition: 2, maxLength: 100),
            CreateColumn("CreatedAt", "datetime", ordinalPosition: 3)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 1);

        // Assert
        result.Rows[0]["Id"].Is("1");
        result.Rows[0]["Name"].Is("1-Fixed-VARCHAR(100)");
        result.Rows[0]["CreatedAt"].Is("2026-05-17 10:30:45");

        sampleDataRepositoryMock.Verify(x => x.GetValues(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void 文字列型は桁数に収まる表記まで短くして値を作成する()
    {
        // Arrange
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Products");
        var columns = new[]
        {
            CreateColumn("FixedFull", "varchar", ordinalPosition: 1, maxLength: 100),
            CreateColumn("FixedShort", "varchar", ordinalPosition: 2, maxLength: 12),
            CreateColumn("FixedVeryShort", "varchar", ordinalPosition: 3, maxLength: 7),
            CreateColumn("FixedMinimum", "varchar", ordinalPosition: 4, maxLength: 1),
            CreateColumn("AdjustableFull", "nvarchar", ordinalPosition: 5, maxLength: 200),
            CreateColumn("AdjustableShort", "nvarchar", ordinalPosition: 6, maxLength: 30),
            CreateColumn("AdjustableVeryShort", "nvarchar", ordinalPosition: 7, maxLength: 24),
            CreateColumn("AdjustableMinimum", "nvarchar", ordinalPosition: 8, maxLength: 2)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 1);

        // Assert
        result.Rows[0]["FixedFull"].Is("1-Fixed-VARCHAR(100)");
        result.Rows[0]["FixedShort"].Is("2-Fixed(12)");
        result.Rows[0]["FixedVeryShort"].Is("3-Fixed");
        result.Rows[0]["FixedMinimum"].Is("4");
        result.Rows[0]["AdjustableFull"].Is("5-Adjustable-NVARCHAR(100)");
        result.Rows[0]["AdjustableShort"].Is("6-Adjustable 15");
        result.Rows[0]["AdjustableVeryShort"].Is("7-Adjustable");
        result.Rows[0]["AdjustableMinimum"].Is("8");
    }

    [TestMethod]
    public void 小数型とバイナリ型は連番を使って値を作成する()
    {
        // Arrange
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Products");
        var columns = new[]
        {
            CreateColumn("Price1", "decimal", ordinalPosition: 1, numericScale: 3),
            CreateColumn("Price2", "numeric", ordinalPosition: 2, numericScale: 3),
            CreateColumn("Image1", "varbinary", ordinalPosition: 3),
            CreateColumn("Image2", "binary", ordinalPosition: 4)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 1);

        // Assert
        result.Rows[0]["Price1"].Is("1.001");
        result.Rows[0]["Price2"].Is("2.001");
        result.Rows[0]["Image1"].Is("0x01");
        result.Rows[0]["Image2"].Is("0x02");
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
        int ordinalPosition,
        int? maxLength = null,
        int? numericScale = null)
    {
        return new DbColumnInfo
        {
            SchemaName = "dbo",
            TableName = "Users",
            ColumnName = columnName,
            DataType = dataType,
            OrdinalPosition = ordinalPosition,
            MaxLength = maxLength,
            NumericScale = numericScale
        };
    }
}
