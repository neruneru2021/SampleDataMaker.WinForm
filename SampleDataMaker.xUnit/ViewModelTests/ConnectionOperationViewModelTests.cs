using ChainingAssertion;
using Moq;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;
using SampleDataMaker.WinForm.ViewModels;
using System.Data;

namespace SampleDataMaker.xUnit.ViewModelTests;

/// <summary>
/// ConnectionOperationViewModelのテーブル選択、カラム表示、外部キー表示更新を確認します。
///
/// このViewModelは依存先が多いですが、単体テストでは全てMoqで差し替えます。
/// そうすることでDBやファイルに触れず、画面ロジックだけを確認できます。
/// </summary>
[TestClass]
public sealed class ConnectionOperationViewModelTests
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
    public async Task Initializeはテーブル一覧とサンプルデータ種類を読み込む()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        fixture.TableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(connection, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTable("PLMCONSOLE", "PLM_CAD_DATA")
            });
        fixture.SampleDataRepositoryMock
            .Setup(x => x.GetKinds())
            .Returns(new[] { "氏名", "都道府県" });

        // Act
        await fixture.ViewModel.Initialize(connection);

        // Assert
        fixture.ViewModel.TablesSource.Count.Is(1);
        fixture.ViewModel.TablesSource[0].SchemaName.Is("PLMCONSOLE");
        fixture.ViewModel.TablesSource[0].TableName.Is("PLM_CAD_DATA");

        // 先頭には「未選択」を表す空文字が入ります。
        fixture.ViewModel.SampleDataKindsSource[0].Is("");
        fixture.ViewModel.SampleDataKindsSource[1].Is("氏名");
        fixture.ViewModel.SampleDataKindsSource[2].Is("都道府県");
    }

    [TestMethod]
    public async Task LoadColumnsは保存済み外部キーの参照先を表示用プロパティへ反映する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var table = CreateTable("PLMCONSOLE", "USERS");
        var tableItem = new DbTableSelectionItem(table);
        fixture.ForeignKeyRelationRepositoryMock
            .Setup(x => x.GetAll())
            .Returns(new[]
            {
                new ForeignKeyRelationSetting
                {
                    SourceSchemaName = "PLMCONSOLE",
                    SourceTableName = "USERS",
                    SourceColumnName = "CLINIC_ID",
                    ReferenceSchemaName = "PLMCONSOLE",
                    ReferenceTableName = "CLINICS",
                    ReferenceColumnName = "CLINIC_ID"
                }
            });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateColumn(table, "CLINIC_ID", "NUMBER", 1)
            });

        await fixture.ViewModel.Initialize(connection);

        // Act
        await fixture.ViewModel.LoadColumns(tableItem);

        // Assert
        fixture.ViewModel.ColumnsSource.Count.Is(1);
        fixture.ViewModel.ColumnsSource[0].ForeignKeyDisplay
            .Is("PLMCONSOLE.CLINICS.CLINIC_ID");
    }

    [TestMethod]
    public async Task SaveForeignKeySettingsは保存後に表示中カラムの外部キー表示も更新する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var table = CreateTable("PLMCONSOLE", "USERS");
        var tableItem = new DbTableSelectionItem(table);
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateColumn(table, "CLINIC_ID", "NUMBER", 1)
            });

        await fixture.ViewModel.Initialize(connection);
        await fixture.ViewModel.LoadColumns(tableItem);
        var columnItem = fixture.ViewModel.ColumnsSource[0];
        var settings = new[]
        {
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "PLMCONSOLE",
                SourceTableName = "USERS",
                SourceColumnName = "CLINIC_ID",
                ReferenceSchemaName = "PLMCONSOLE",
                ReferenceTableName = "CLINICS",
                ReferenceColumnName = "CLINIC_ID"
            }
        };

        // Act
        await fixture.ViewModel.SaveForeignKeySettings(columnItem, settings);

        // Assert
        columnItem.ForeignKeyDisplay.Is("PLMCONSOLE.CLINICS.CLINIC_ID");
        fixture.ForeignKeyRelationRepositoryMock.Verify(
            x => x.SaveAllAsync(
                It.Is<IReadOnlyList<ForeignKeyRelationSetting>>(items => items.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static TestFixture CreateFixture()
    {
        var tableInfoRepositoryMock = new Mock<IDbTableInfoRepository>();
        var tableSchemaRepositoryMock = new Mock<IDbTableSchemaRepository>();
        var testDataGeneratorMock = new Mock<ITestDataGenerator>();
        var boundaryTestDataGeneratorMock = new Mock<IBoundaryTestDataGenerator>();
        var outputRepositoryMock = new Mock<ITestDataOutputRepository>();
        var sampleDataRepositoryMock = new Mock<ISampleDataRepository>();
        var templateRepositoryMock = new Mock<IColumnSampleDataTemplateRepository>();
        var foreignKeyRelationRepositoryMock = new Mock<IForeignKeyRelationRepository>();
        var foreignKeyTestDataApplierMock = new Mock<IForeignKeyTestDataApplier>();

        tableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(It.IsAny<DbConnectionInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DbTableInfo>());
        sampleDataRepositoryMock
            .Setup(x => x.GetKinds())
            .Returns(Array.Empty<string>());
        templateRepositoryMock
            .Setup(x => x.GetAll())
            .Returns(Array.Empty<ColumnSampleDataTemplate>());
        foreignKeyRelationRepositoryMock
            .Setup(x => x.GetAll())
            .Returns(Array.Empty<ForeignKeyRelationSetting>());
        foreignKeyRelationRepositoryMock
            .Setup(x => x.SaveAllAsync(It.IsAny<IReadOnlyList<ForeignKeyRelationSetting>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var vm = new ConnectionOperationViewModel(
            tableInfoRepositoryMock.Object,
            tableSchemaRepositoryMock.Object,
            testDataGeneratorMock.Object,
            boundaryTestDataGeneratorMock.Object,
            outputRepositoryMock.Object,
            sampleDataRepositoryMock.Object,
            templateRepositoryMock.Object,
            foreignKeyRelationRepositoryMock.Object,
            foreignKeyTestDataApplierMock.Object);

        return new TestFixture(
            vm,
            tableInfoRepositoryMock,
            tableSchemaRepositoryMock,
            sampleDataRepositoryMock,
            foreignKeyRelationRepositoryMock);
    }

    private static DbConnectionInfo CreateConnection()
    {
        return new DbConnectionInfo
        {
            Title = "Oracle PLM",
            DbType = DbTypeKind.Oracle,
            ConnectionString = "User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;",
            DefaultSchema = "PLMCONSOLE"
        };
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
        DbTableInfo table,
        string columnName,
        string dataType,
        int ordinalPosition)
    {
        return new DbColumnInfo
        {
            SchemaName = table.SchemaName,
            TableName = table.TableName,
            ColumnName = columnName,
            DataType = dataType,
            OrdinalPosition = ordinalPosition
        };
    }

    private sealed record TestFixture(
        ConnectionOperationViewModel ViewModel,
        Mock<IDbTableInfoRepository> TableInfoRepositoryMock,
        Mock<IDbTableSchemaRepository> TableSchemaRepositoryMock,
        Mock<ISampleDataRepository> SampleDataRepositoryMock,
        Mock<IForeignKeyRelationRepository> ForeignKeyRelationRepositoryMock);
}
