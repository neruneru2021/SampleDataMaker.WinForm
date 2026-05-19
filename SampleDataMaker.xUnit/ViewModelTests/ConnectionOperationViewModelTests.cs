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
                It.Is<IReadOnlyList<ForeignKeyRelationSetting>>(items =>
                    items.Count == 2
                    && items.Any(setting =>
                        setting.SourceSchemaName == "PLMCONSOLE"
                        && setting.SourceTableName == "USERS"
                        && setting.SourceColumnName == "CLINIC_ID"
                        && setting.ReferenceSchemaName == "PLMCONSOLE"
                        && setting.ReferenceTableName == "CLINICS"
                        && setting.ReferenceColumnName == "CLINIC_ID")
                    && items.Any(setting =>
                        setting.SourceSchemaName == "PLMCONSOLE"
                        && setting.SourceTableName == "CLINICS"
                        && setting.SourceColumnName == "CLINIC_ID"
                        && setting.ReferenceSchemaName == "PLMCONSOLE"
                        && setting.ReferenceTableName == "USERS"
                        && setting.ReferenceColumnName == "CLINIC_ID")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SaveForeignKeySettingsは操作中カラムの種類を参照先カラムにも反映する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var usersTable = CreateTable("PLMCONSOLE", "USERS");
        var clinicsTable = CreateTable("PLMCONSOLE", "CLINICS");
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, usersTable, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateColumn(usersTable, "CLINIC_ID", "NUMBER", 1)
            });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(
                connection,
                It.Is<DbTableInfo>(table =>
                    table.SchemaName == clinicsTable.SchemaName
                    && table.TableName == clinicsTable.TableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateColumn(clinicsTable, "CLINIC_ID", "NUMBER", 1)
            });

        await fixture.ViewModel.Initialize(connection);
        await fixture.ViewModel.LoadColumns(new DbTableSelectionItem(usersTable));
        var columnItem = fixture.ViewModel.ColumnsSource[0];
        columnItem.SampleDataKind = "病院ID";
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
        await fixture.ViewModel.LoadColumns(new DbTableSelectionItem(clinicsTable));

        // Assert
        fixture.ViewModel.ColumnsSource[0].SampleDataKind.Is("病院ID");
    }

    [TestMethod]
    public async Task SaveForeignKeySettingsは解除時に双方向の外部キー設定を削除する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var usersTable = CreateTable("PLMCONSOLE", "USERS");
        var tableItem = new DbTableSelectionItem(usersTable);
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
                },
                new ForeignKeyRelationSetting
                {
                    SourceSchemaName = "PLMCONSOLE",
                    SourceTableName = "CLINICS",
                    SourceColumnName = "CLINIC_ID",
                    ReferenceSchemaName = "PLMCONSOLE",
                    ReferenceTableName = "USERS",
                    ReferenceColumnName = "CLINIC_ID"
                },
                new ForeignKeyRelationSetting
                {
                    SourceSchemaName = "PLMCONSOLE",
                    SourceTableName = "USERS",
                    SourceColumnName = "DEPARTMENT_ID",
                    ReferenceSchemaName = "PLMCONSOLE",
                    ReferenceTableName = "DEPARTMENTS",
                    ReferenceColumnName = "DEPARTMENT_ID"
                }
            });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, usersTable, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateColumn(usersTable, "CLINIC_ID", "NUMBER", 1)
            });

        await fixture.ViewModel.Initialize(connection);
        await fixture.ViewModel.LoadColumns(tableItem);
        var columnItem = fixture.ViewModel.ColumnsSource[0];

        // Act
        await fixture.ViewModel.SaveForeignKeySettings(
            columnItem,
            Array.Empty<ForeignKeyRelationSetting>());

        // Assert
        columnItem.ForeignKeyDisplay.Is("");
        fixture.ForeignKeyRelationRepositoryMock.Verify(
            x => x.SaveAllAsync(
                It.Is<IReadOnlyList<ForeignKeyRelationSetting>>(items =>
                    items.Count == 1
                    && items[0].SourceTableName == "USERS"
                    && items[0].SourceColumnName == "DEPARTMENT_ID"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateTestDataはDirect指定なしの場合ファイル出力Repositoryへ保存する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var table = CreateTable("PLMCONSOLE", "USERS");
        fixture.TableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(connection, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { table });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateColumn(table, "USER_ID", "NUMBER", 1) });

        var generatedData = new GeneratedTestData(
            table,
            new[] { CreateColumn(table, "USER_ID", "NUMBER", 1) },
            new[] { Row(("USER_ID", "1")) });
        fixture.TestDataGeneratorMock
            .Setup(x => x.Generate(
                table,
                It.IsAny<IReadOnlyList<DbColumnInfo>>(),
                It.IsAny<IReadOnlyList<ColumnSampleDataSetting>>(),
                1,
                It.IsAny<IReadOnlyDictionary<string, int>>()))
            .Returns(generatedData);
        fixture.ForeignKeyTestDataApplierMock
            .Setup(x => x.Apply(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<IReadOnlyList<ForeignKeyRelationSetting>>()))
            .Returns((IReadOnlyList<GeneratedTestData> testDataList, IReadOnlyList<ForeignKeyRelationSetting> _) => testDataList);
        fixture.OutputRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestDataOutputResult("file-output", Array.Empty<string>()));

        await fixture.ViewModel.Initialize(connection);
        fixture.ViewModel.TablesSource[0].IsSelected = true;

        // Act
        var result = await fixture.ViewModel.CreateTestData(rowCount: 1, directInsert: false);

        // Assert
        result.OutputDirectoryPath.Is("file-output");
        fixture.OutputRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.DirectInsertRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<DbConnectionInfo>(), It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.ExistingKeyValueRepositoryMock.Verify(
            x => x.GetMaxValuesAsync(
                It.IsAny<DbConnectionInfo>(),
                It.IsAny<DbTableInfo>(),
                It.IsAny<IReadOnlyList<DbColumnInfo>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task CreateTestDataはDirect指定ありの場合DB直接登録Repositoryへ保存する()
    {
        // Arrange
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var table = CreateTable("PLMCONSOLE", "USERS");
        fixture.TableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(connection, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { table });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateColumn(table, "USER_ID", "NUMBER", 1) });

        var generatedData = new GeneratedTestData(
            table,
            new[] { CreateColumn(table, "USER_ID", "NUMBER", 1) },
            new[] { Row(("USER_ID", "1")) });
        fixture.TestDataGeneratorMock
            .Setup(x => x.Generate(
                table,
                It.IsAny<IReadOnlyList<DbColumnInfo>>(),
                It.IsAny<IReadOnlyList<ColumnSampleDataSetting>>(),
                1,
                It.IsAny<IReadOnlyDictionary<string, int>>()))
            .Returns(generatedData);
        fixture.ForeignKeyTestDataApplierMock
            .Setup(x => x.Apply(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<IReadOnlyList<ForeignKeyRelationSetting>>()))
            .Returns((IReadOnlyList<GeneratedTestData> testDataList, IReadOnlyList<ForeignKeyRelationSetting> _) => testDataList);
        fixture.DirectInsertRepositoryMock
            .Setup(x => x.SaveAsync(connection, It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestDataOutputResult("direct-output", Array.Empty<string>()));

        await fixture.ViewModel.Initialize(connection);
        fixture.ViewModel.TablesSource[0].IsSelected = true;

        // Act
        var result = await fixture.ViewModel.CreateTestData(rowCount: 1, directInsert: true);

        // Assert
        result.OutputDirectoryPath.Is("direct-output");
        fixture.DirectInsertRepositoryMock.Verify(
            x => x.SaveAsync(connection, It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.OutputRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task CreateTestDataはDirect指定ありの場合既存キー最大値を生成処理へ渡す()
    {
        // Arrange
        // 直接INSERTでは、既存テーブルに続けてデータを追加します。
        // そのため、ViewModelはDB上の現在最大値を取得し、
        // Generatorへ「ここから続けて採番してね」という開始位置を渡します。
        var fixture = CreateFixture();
        var connection = CreateConnection();
        var table = CreateTable("PLMCONSOLE", "USERS");
        var userIdColumn = CreateColumn(table, "USER_ID", "NUMBER", 1);
        var startNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["PLMCONSOLE.USERS.USER_ID"] = 100
        };
        var generatedData = new GeneratedTestData(
            table,
            new[] { userIdColumn },
            new[] { Row(("USER_ID", "101")) });

        fixture.TableInfoRepositoryMock
            .Setup(x => x.GetTablesAsync(connection, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { table });
        fixture.TableSchemaRepositoryMock
            .Setup(x => x.GetColumnsAsync(connection, table, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { userIdColumn });
        fixture.ExistingKeyValueRepositoryMock
            .Setup(x => x.GetMaxValuesAsync(connection, table, It.IsAny<IReadOnlyList<DbColumnInfo>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(startNumbers);
        fixture.TestDataGeneratorMock
            .Setup(x => x.Generate(
                table,
                It.IsAny<IReadOnlyList<DbColumnInfo>>(),
                It.IsAny<IReadOnlyList<ColumnSampleDataSetting>>(),
                1,
                It.Is<IReadOnlyDictionary<string, int>>(values =>
                    values.ContainsKey("PLMCONSOLE.USERS.USER_ID")
                    && values["PLMCONSOLE.USERS.USER_ID"] == 100)))
            .Returns(generatedData);
        fixture.ForeignKeyTestDataApplierMock
            .Setup(x => x.Apply(It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<IReadOnlyList<ForeignKeyRelationSetting>>()))
            .Returns((IReadOnlyList<GeneratedTestData> testDataList, IReadOnlyList<ForeignKeyRelationSetting> _) => testDataList);
        fixture.DirectInsertRepositoryMock
            .Setup(x => x.SaveAsync(connection, It.IsAny<IReadOnlyList<GeneratedTestData>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestDataOutputResult("direct-output", Array.Empty<string>()));

        await fixture.ViewModel.Initialize(connection);
        fixture.ViewModel.TablesSource[0].IsSelected = true;

        // Act
        await fixture.ViewModel.CreateTestData(rowCount: 1, directInsert: true);

        // Assert
        fixture.ExistingKeyValueRepositoryMock.Verify(
            x => x.GetMaxValuesAsync(connection, table, It.IsAny<IReadOnlyList<DbColumnInfo>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static TestFixture CreateFixture()
    {
        var tableInfoRepositoryMock = new Mock<IDbTableInfoRepository>();
        var tableSchemaRepositoryMock = new Mock<IDbTableSchemaRepository>();
        var testDataGeneratorMock = new Mock<ITestDataGenerator>();
        var boundaryTestDataGeneratorMock = new Mock<IBoundaryTestDataGenerator>();
        var outputRepositoryMock = new Mock<ITestDataOutputRepository>();
        var directInsertRepositoryMock = new Mock<ITestDataDirectInsertRepository>();
        var existingKeyValueRepositoryMock = new Mock<IExistingKeyValueRepository>();
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
        existingKeyValueRepositoryMock
            .Setup(x => x.GetMaxValuesAsync(
                It.IsAny<DbConnectionInfo>(),
                It.IsAny<DbTableInfo>(),
                It.IsAny<IReadOnlyList<DbColumnInfo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));

        var vm = new ConnectionOperationViewModel(
            tableInfoRepositoryMock.Object,
            tableSchemaRepositoryMock.Object,
            testDataGeneratorMock.Object,
            boundaryTestDataGeneratorMock.Object,
            outputRepositoryMock.Object,
            directInsertRepositoryMock.Object,
            existingKeyValueRepositoryMock.Object,
            sampleDataRepositoryMock.Object,
            templateRepositoryMock.Object,
            foreignKeyRelationRepositoryMock.Object,
            foreignKeyTestDataApplierMock.Object);

        return new TestFixture(
            vm,
            tableInfoRepositoryMock,
            tableSchemaRepositoryMock,
            testDataGeneratorMock,
            outputRepositoryMock,
            directInsertRepositoryMock,
            existingKeyValueRepositoryMock,
            sampleDataRepositoryMock,
            foreignKeyRelationRepositoryMock,
            foreignKeyTestDataApplierMock);
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

    private static IReadOnlyDictionary<string, string?> Row(params (string ColumnName, string? Value)[] values)
    {
        return values.ToDictionary(x => x.ColumnName, x => x.Value);
    }

    private sealed record TestFixture(
        ConnectionOperationViewModel ViewModel,
        Mock<IDbTableInfoRepository> TableInfoRepositoryMock,
        Mock<IDbTableSchemaRepository> TableSchemaRepositoryMock,
        Mock<ITestDataGenerator> TestDataGeneratorMock,
        Mock<ITestDataOutputRepository> OutputRepositoryMock,
        Mock<ITestDataDirectInsertRepository> DirectInsertRepositoryMock,
        Mock<IExistingKeyValueRepository> ExistingKeyValueRepositoryMock,
        Mock<ISampleDataRepository> SampleDataRepositoryMock,
        Mock<IForeignKeyRelationRepository> ForeignKeyRelationRepositoryMock,
        Mock<IForeignKeyTestDataApplier> ForeignKeyTestDataApplierMock);
}
