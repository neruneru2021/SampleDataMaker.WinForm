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
    private DbConnectionInfo? _connection;

    private BindingList<DbTableSelectionItem> _tablesSource = new();

    public BindingList<DbTableSelectionItem> TablesSource
    {
        get => _tablesSource;
        private set => SetProperty(ref _tablesSource, value);
    }

    public ConnectionOperationViewModel(
        IDbTableInfoRepository dbTableInfoRepository,
        IDbTableSchemaRepository dbTableSchemaRepository,
        ITestDataGenerator testDataGenerator,
        IBoundaryTestDataGenerator boundaryTestDataGenerator,
        ITestDataOutputRepository testDataOutputRepository)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
        _dbTableSchemaRepository = dbTableSchemaRepository;
        _testDataGenerator = testDataGenerator;
        _boundaryTestDataGenerator = boundaryTestDataGenerator;
        _testDataOutputRepository = testDataOutputRepository;
    }

    public async Task Initialize(DbConnectionInfo connection)
    {
        _connection = connection;

        var tables = await _dbTableInfoRepository.GetTablesAsync(connection);

        TablesSource = new BindingList<DbTableSelectionItem>(
            tables.Select(table => new DbTableSelectionItem(table)).ToList());
    }

    public async Task<TestDataOutputResult> CreateTestData()
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
            var testData = _testDataGenerator.Generate(table, columns);

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
            var testData = _boundaryTestDataGenerator.Generate(table, columns);

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
}
