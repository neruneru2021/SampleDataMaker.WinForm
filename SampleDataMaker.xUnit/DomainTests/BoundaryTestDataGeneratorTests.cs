using ChainingAssertion;
using Moq;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;

namespace SampleDataMaker.xUnit.DomainTests;

/// <summary>
/// 境界値テストデータ生成の代表的な振る舞いを確認します。
///
/// 境界値生成は「型ごとの最小値・最大値」と「Nullableならnull」
/// 「文字列なら空文字も試す」というように、仕様が複数あります。
/// ここでは、SQL Server型だけでなくOracle型も同じ考え方で扱えることを確認します。
/// </summary>
[TestClass]
public sealed class BoundaryTestDataGeneratorTests
{
    [TestMethod]
    public void OracleのNUMBER型は精度と小数桁から最大値を作成する()
    {
        // Arrange
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new BoundaryTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable();
        var columns = new[]
        {
            new DbColumnInfo
            {
                SchemaName = "PLMCONSOLE",
                TableName = "PRODUCTS",
                ColumnName = "PRICE",
                DataType = "NUMBER",
                OrdinalPosition = 1,
                NumericPrecision = 5,
                NumericScale = 2
            }
        };

        // Act
        var result = generator.Generate(table, columns);

        // Assert
        // NUMBER(5, 2) の最大値は整数部3桁 + 小数部2桁なので 999.99 です。
        result.Rows.Count.Is(2);
        result.Rows[0]["PRICE"].Is("0");
        result.Rows[1]["PRICE"].Is("999.99");
    }

    [TestMethod]
    public void Nullable文字列型は最小値最大値null空文字の行を作成する()
    {
        // Arrange
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new BoundaryTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable();
        var columns = new[]
        {
            new DbColumnInfo
            {
                SchemaName = "PLMCONSOLE",
                TableName = "PRODUCTS",
                ColumnName = "PRODUCT_NAME",
                DataType = "VARCHAR2",
                OrdinalPosition = 1,
                IsNullable = true,
                MaxLength = 3
            }
        };

        // Act
        var result = generator.Generate(table, columns);

        // Assert
        result.Rows.Count.Is(4);
        result.Rows[0]["PRODUCT_NAME"].Is("");
        result.Rows[1]["PRODUCT_NAME"].Is("ZZZ");
        result.Rows[2]["PRODUCT_NAME"].Is((string?)null);
        result.Rows[3]["PRODUCT_NAME"].Is("");
    }

    [TestMethod]
    public void Randomが選択されても境界値生成ではマスタ値もランダム値も使わない()
    {
        // Arrange
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var generator = new BoundaryTestDataGenerator(sampleDataRepositoryMock.Object);
        var table = CreateTable();
        var columns = new[]
        {
            new DbColumnInfo
            {
                SchemaName = "PLMCONSOLE",
                TableName = "PRODUCTS",
                ColumnName = "PRODUCT_NAME",
                DataType = "VARCHAR2",
                OrdinalPosition = 1,
                MaxLength = 3
            }
        };
        var settings = new[]
        {
            new ColumnSampleDataSetting
            {
                ColumnName = "PRODUCT_NAME",
                SampleDataKind = SampleDataKindNames.Random
            }
        };

        // Act
        var result = generator.Generate(table, columns, settings);

        // Assert
        result.Rows.Count.Is(3);
        result.Rows[0]["PRODUCT_NAME"].Is("");
        result.Rows[1]["PRODUCT_NAME"].Is("ZZZ");
        result.Rows[2]["PRODUCT_NAME"].Is("");
        sampleDataRepositoryMock.Verify(x => x.GetValues(It.IsAny<string>()), Times.Never);
    }

    private static DbTableInfo CreateTable()
    {
        return new DbTableInfo
        {
            SchemaName = "PLMCONSOLE",
            TableName = "PRODUCTS"
        };
    }
}
