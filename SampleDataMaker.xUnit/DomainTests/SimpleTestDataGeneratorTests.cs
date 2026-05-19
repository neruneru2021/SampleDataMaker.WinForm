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
        result.Rows[0]["FixedShort"].Is("1-Fixed(12)");
        result.Rows[0]["FixedVeryShort"].Is("1-Fixed");
        result.Rows[0]["FixedMinimum"].Is("1");
        result.Rows[0]["AdjustableFull"].Is("1-Adjustable-NVARCHAR(100)");
        result.Rows[0]["AdjustableShort"].Is("1-Adjustable 15");
        result.Rows[0]["AdjustableVeryShort"].Is("1-Adjustable");
        result.Rows[0]["AdjustableMinimum"].Is("1");
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
        result.Rows[0]["Price2"].Is("1.001");
        result.Rows[0]["Image1"].Is("0x01");
        result.Rows[0]["Image2"].Is("0x01");
    }

    [TestMethod]
    public void 整数型は型の上限を超えたら1から振り直す()
    {
        // Arrange
        // tinyintはSQL Serverでは0〜255の範囲です。
        // このツールの通常生成は1から始めるため、256行目は1へ折り返す必要があります。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Products");
        var columns = new[]
        {
            CreateColumn("Ktype", "tinyint", ordinalPosition: 1)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 256);

        // Assert
        result.Rows[0]["Ktype"].Is("1");
        result.Rows[254]["Ktype"].Is("255");
        result.Rows[255]["Ktype"].Is("1");
    }

    [TestMethod]
    public void 開始番号が指定された数値カラムは既存最大値の次から値を作成する()
    {
        // Arrange
        // 直接INSERTで既存データへ追加する時は、DB上の最大値を開始番号として渡します。
        // ここでは既に100まで入っている想定なので、次に作る値は101から始まります。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Users");
        var columns = new[]
        {
            CreateColumn("UserId", "int", ordinalPosition: 1)
        };
        var startNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["dbo.Users.UserId"] = 100
        };

        // Act
        var result = generator.Generate(
            table,
            columns,
            rowCount: 2,
            columnStartNumbers: startNumbers);

        // Assert
        result.Rows[0]["UserId"].Is("101");
        result.Rows[1]["UserId"].Is("102");
    }

    [TestMethod]
    public void 開始番号が指定された数値カラムは型の上限を超える場合に例外を出す()
    {
        // Arrange
        // tinyintは255が上限です。
        // 既存最大値が255の場合、1へ折り返すと既存キーと衝突する可能性が高いため、
        // 追加作成モードでは自動折り返しではなく、理由が分かる例外にします。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Users");
        var columns = new[]
        {
            CreateColumn("Ktype", "tinyint", ordinalPosition: 1)
        };
        var startNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["dbo.Users.Ktype"] = 255
        };

        // Act
        var action = () => generator.Generate(
            table,
            columns,
            rowCount: 1,
            columnStartNumbers: startNumbers);

        // Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(action);
        exception.Message.Contains("キー重複を避けて追加作成できません").IsTrue();
    }

    [TestMethod]
    public void 文字列型は桁数を超える前に1から振り直す()
    {
        // Arrange
        // nchar(1)のような1文字カラムでは、10という値は2文字になるため入れられません。
        // そのため9の次は1へ折り返します。
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new SimpleTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable("dbo", "Products");
        var columns = new[]
        {
            CreateColumn("ProductName", "nchar", ordinalPosition: 1, maxLength: 2)
        };

        // Act
        var result = generator.Generate(table, columns, rowCount: 10);

        // Assert
        result.Rows[0]["ProductName"].Is("1");
        result.Rows[8]["ProductName"].Is("9");
        result.Rows[9]["ProductName"].Is("1");
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
