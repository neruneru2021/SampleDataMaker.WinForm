using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;
using System.ComponentModel;

namespace SampleDataMaker.WinForm.ViewModels;

internal class ConnectionOperationViewModel : ViewModelBase
{
    private readonly IDbTableInfoRepository _dbTableInfoRepository;
    private readonly IDbTableSchemaRepository _dbTableSchemaRepository;
    private readonly ITestDataGenerator _testDataGenerator;
    private readonly IBoundaryTestDataGenerator _boundaryTestDataGenerator;
    private readonly ITestDataOutputRepository _testDataOutputRepository;
    private readonly ISampleDataRepository _sampleDataRepository;
    private readonly IColumnSampleDataTemplateRepository _templateRepository;
    private readonly Dictionary<string, BindingList<DbColumnSampleDataSelectionItem>> _columnsByTable = new();
    private DbConnectionInfo? _connection;
    private DbTableInfo? _currentTable;

    private BindingList<DbTableSelectionItem> _tablesSource = new();
    private BindingList<DbColumnSampleDataSelectionItem> _columnsSource = new();
    private BindingList<string> _sampleDataKindsSource = new();

    public BindingList<DbTableSelectionItem> TablesSource
    {
        get => _tablesSource;
        private set => SetProperty(ref _tablesSource, value);
    }

    public BindingList<DbColumnSampleDataSelectionItem> ColumnsSource
    {
        get => _columnsSource;
        private set => SetProperty(ref _columnsSource, value);
    }

    public BindingList<string> SampleDataKindsSource
    {
        get => _sampleDataKindsSource;
        private set => SetProperty(ref _sampleDataKindsSource, value);
    }

    public ConnectionOperationViewModel(
        IDbTableInfoRepository dbTableInfoRepository,
        IDbTableSchemaRepository dbTableSchemaRepository,
        ITestDataGenerator testDataGenerator,
        IBoundaryTestDataGenerator boundaryTestDataGenerator,
        ITestDataOutputRepository testDataOutputRepository,
        ISampleDataRepository sampleDataRepository,
        IColumnSampleDataTemplateRepository templateRepository)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
        _dbTableSchemaRepository = dbTableSchemaRepository;
        _testDataGenerator = testDataGenerator;
        _boundaryTestDataGenerator = boundaryTestDataGenerator;
        _testDataOutputRepository = testDataOutputRepository;
        _sampleDataRepository = sampleDataRepository;
        _templateRepository = templateRepository;
    }

    public async Task Initialize(DbConnectionInfo connection)
    {
        _connection = connection;

        var tables = await _dbTableInfoRepository.GetTablesAsync(connection);

        TablesSource = new BindingList<DbTableSelectionItem>(
            tables.Select(table => new DbTableSelectionItem(table)).ToList());

        SampleDataKindsSource.Clear();
        SampleDataKindsSource.Add(string.Empty);

        foreach (var kind in _sampleDataRepository.GetKinds())
        {
            SampleDataKindsSource.Add(kind);
        }
    }

    public async Task LoadColumns(DbTableSelectionItem? tableItem)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        if (tableItem == null)
        {
            return;
        }

        _currentTable = tableItem.Table;

        var key = CreateTableKey(tableItem.Table);
        if (!_columnsByTable.TryGetValue(key, out var columnsSource))
        {
            var columns = await _dbTableSchemaRepository.GetColumnsAsync(_connection, tableItem.Table);
            columnsSource = new BindingList<DbColumnSampleDataSelectionItem>(
                columns.Select(column => new DbColumnSampleDataSelectionItem(column)).ToList());

            _columnsByTable.Add(key, columnsSource);
        }

        ColumnsSource = columnsSource;
    }

    public async Task SaveCurrentTemplate(string templateName)
    {
        if (_currentTable == null)
        {
            throw new InvalidOperationException("テンプレートを保存するテーブルを選択してください。");
        }

        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new InvalidOperationException("テンプレート名を入力してください。");
        }

        var template = new ColumnSampleDataTemplate
        {
            TemplateName = templateName.Trim(),
            SchemaName = _currentTable.SchemaName,
            TableName = _currentTable.TableName,
            Columns = ColumnsSource.Select(column => column.ToSetting()).ToList()
        };

        await _templateRepository.SaveAsync(template);
    }

    public async Task<TestDataOutputResult> CreateTestData(int rowCount)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        if (rowCount <= 0)
        {
            throw new InvalidOperationException("作成件数は1以上の数値を入力してください。");
        }

        var selectedTables = GetSelectedTables();
        var testDataList = new List<GeneratedTestData>();

        foreach (var table in selectedTables)
        {
            var columns = await _dbTableSchemaRepository.GetColumnsAsync(_connection, table);
            var testData = _testDataGenerator.Generate(
                table,
                columns,
                GetSampleDataSettings(table),
                rowCount);

            testDataList.Add(testData);
        }

        return await _testDataOutputRepository.SaveAsync(testDataList);
    }

    public async Task<TestDataOutputResult> CreateBoundaryTestData()
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        var selectedTables = GetSelectedTables();
        var testDataList = new List<GeneratedTestData>();

        foreach (var table in selectedTables)
        {
            var columns = await _dbTableSchemaRepository.GetColumnsAsync(_connection, table);
            var testData = _boundaryTestDataGenerator.Generate(
                table,
                columns,
                GetSampleDataSettings(table));

            testDataList.Add(testData);
        }

        return await _testDataOutputRepository.SaveAsync(testDataList);
    }

    private List<DbTableInfo> GetSelectedTables()
    {
        var selectedTables = TablesSource
            .Where(item => item.IsSelected)
            .Select(item => item.Table)
            .ToList();

        if (selectedTables.Count == 0)
        {
            throw new InvalidOperationException("テストデータを作成するテーブルを選択してください。");
        }

        return selectedTables;
    }

    private IReadOnlyList<ColumnSampleDataSetting> GetSampleDataSettings(DbTableInfo table)
    {
        var key = CreateTableKey(table);
        if (!_columnsByTable.TryGetValue(key, out var columnsSource))
        {
            return Array.Empty<ColumnSampleDataSetting>();
        }

        return columnsSource
            .Select(column => column.ToSetting())
            .ToList();
    }

    private static string CreateTableKey(DbTableInfo table)
    {
        return $"{table.SchemaName}.{table.TableName}";
    }
}
