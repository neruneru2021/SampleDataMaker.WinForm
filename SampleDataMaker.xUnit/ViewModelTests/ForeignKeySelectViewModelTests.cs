using ChainingAssertion;
using Moq;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.WinForm.Services;
using SampleDataMaker.WinForm.ViewModels;
using System.Data;

namespace SampleDataMaker.xUnit.ViewModelTests;

/// <summary>
/// ForeignKeySelectViewModelの外部キー追加ロジックを確認します。
///
/// 今回の重要ポイントは、設定元カラムと参照先カラムのデータ型が異なる場合です。
/// 実画面ではMessageBoxで確認しますが、単体テストでは確認サービスをMoqで差し替え、
/// 「警告が出ること」と「利用者の選択によって追加可否が変わること」を検証します。
/// </summary>
[TestClass]
public sealed class ForeignKeySelectViewModelTests
{
    [TestInitialize]
    public void Initialize()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [TestCleanup]
    public void Cleanup()
    {
        SynchronizationContext.SetSynchronizationContext(null);
    }

    [TestMethod]
    public async Task 同じデータ型の参照先カラムは確認なしで外部キー候補に追加する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var sourceColumn = CreateColumn("Product", "ProductNo", "float");
        var referenceColumn = CreateColumn("SubProduct", "ProductNo", "float");

        await fixture.ViewModel.Initialize(
            connection,
            sourceColumn,
            Array.Empty<ForeignKeyRelationSetting>());

        // Act
        fixture.ViewModel.AddForeignKey(new DbColumnSampleDataSelectionItem(referenceColumn));

        // Assert
        fixture.ViewModel.ForeignKeySource.Count.Is(1);
        fixture.ViewModel.ForeignKeySource[0].TableName.Is("SubProduct");
        fixture.ViewModel.ForeignKeySource[0].ColumnName.Is("ProductNo");

        // 型が同じなので、警告確認サービスは呼ばれません。
        fixture.TypeMismatchConfirmationServiceMock.Verify(
            x => x.Confirm(It.IsAny<DbColumnInfo>(), It.IsAny<DbColumnInfo>()),
            Times.Never);
    }

    [TestMethod]
    public async Task データ型が異なり確認でキャンセルした場合は外部キー候補に追加しない()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var sourceColumn = CreateColumn("Product", "ProductNo", "float");
        var referenceColumn = CreateColumn("SubProduct", "Name", "nvarchar");
        fixture.TypeMismatchConfirmationServiceMock
            .Setup(x => x.Confirm(sourceColumn, referenceColumn))
            .Returns(false);

        await fixture.ViewModel.Initialize(
            connection,
            sourceColumn,
            Array.Empty<ForeignKeyRelationSetting>());

        // Act
        fixture.ViewModel.AddForeignKey(new DbColumnSampleDataSelectionItem(referenceColumn));

        // Assert
        fixture.ViewModel.ForeignKeySource.Count.Is(0);
        fixture.TypeMismatchConfirmationServiceMock.Verify(
            x => x.Confirm(sourceColumn, referenceColumn),
            Times.Once);
    }

    [TestMethod]
    public async Task データ型が異なり確認で続行した場合は外部キー候補に追加する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var sourceColumn = CreateColumn("Product", "ProductNo", "float");
        var referenceColumn = CreateColumn("SubProduct", "Name", "nvarchar");
        fixture.TypeMismatchConfirmationServiceMock
            .Setup(x => x.Confirm(sourceColumn, referenceColumn))
            .Returns(true);

        await fixture.ViewModel.Initialize(
            connection,
            sourceColumn,
            Array.Empty<ForeignKeyRelationSetting>());

        // Act
        fixture.ViewModel.AddForeignKey(new DbColumnSampleDataSelectionItem(referenceColumn));

        // Assert
        fixture.ViewModel.ForeignKeySource.Count.Is(1);
        fixture.ViewModel.ForeignKeySource[0].TableName.Is("SubProduct");
        fixture.ViewModel.ForeignKeySource[0].ColumnName.Is("Name");
        fixture.TypeMismatchConfirmationServiceMock.Verify(
            x => x.Confirm(sourceColumn, referenceColumn),
            Times.Once);
    }

    private static TestFixture CreateFixture()
    {
        var tableInfoRepositoryMock = new Mock<IDbTableInfoRepository>();
        var tableSchemaRepositoryMock = new Mock<IDbTableSchemaRepository>();
        var typeMismatchConfirmationServiceMock = new Mock<IForeignKeyTypeMismatchConfirmationService>();

        tableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(It.IsAny<DbConnectionInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DbTableInfo>());
        tableInfoRepositoryMock
            .Setup(x => x.GetPreviewDataAsync(It.IsAny<DbConnectionInfo>(), It.IsAny<DbTableInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataTable());

        var vm = new ForeignKeySelectViewModel(
            tableInfoRepositoryMock.Object,
            tableSchemaRepositoryMock.Object,
            typeMismatchConfirmationServiceMock.Object);

        return new TestFixture(vm, typeMismatchConfirmationServiceMock);
    }

    private static DbConnectionInfo CreateConnection()
    {
        return new DbConnectionInfo
        {
            Title = "SQL Server Sample",
            DbType = DbTypeKind.SqlServer,
            ConnectionString = "Server=localhost\\SQLEXPRESS;Database=Sample;"
        };
    }

    private static DbColumnInfo CreateColumn(
        string tableName,
        string columnName,
        string dataType)
    {
        return new DbColumnInfo
        {
            SchemaName = "dbo",
            TableName = tableName,
            ColumnName = columnName,
            DataType = dataType,
            OrdinalPosition = 1
        };
    }

    private sealed record TestFixture(
        ForeignKeySelectViewModel ViewModel,
        Mock<IForeignKeyTypeMismatchConfirmationService> TypeMismatchConfirmationServiceMock);
}
