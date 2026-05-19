using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;
using System.ComponentModel;
using System.Data;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// テーブル操作画面のテーブル選択、カラム設定、プレビュー、テストデータ作成を管理します。
/// </summary>
internal class ConnectionOperationViewModel : ViewModelBase
{
    private readonly IDbTableInfoRepository _dbTableInfoRepository;
    private readonly IDbTableSchemaRepository _dbTableSchemaRepository;
    private readonly ITestDataGenerator _testDataGenerator;
    private readonly IBoundaryTestDataGenerator _boundaryTestDataGenerator;
    private readonly ITestDataOutputRepository _testDataOutputRepository;
    private readonly ITestDataDirectInsertRepository _testDataDirectInsertRepository;
    private readonly ISampleDataRepository _sampleDataRepository;
    private readonly IColumnSampleDataTemplateRepository _templateRepository;
    private readonly IForeignKeyRelationRepository _foreignKeyRelationRepository;
    private readonly IForeignKeyTestDataApplier _foreignKeyTestDataApplier;
    private readonly Dictionary<string, BindingList<DbColumnSampleDataSelectionItem>> _columnsByTable = new();
    private List<ForeignKeyRelationSetting> _foreignKeySettings = new();
    private DbConnectionInfo? _connection;
    private DbTableInfo? _currentTable;

    private BindingList<DbTableSelectionItem> _tablesSource = new();
    private BindingList<DbColumnSampleDataSelectionItem> _columnsSource = new();
    private BindingList<string> _sampleDataKindsSource = new();
    private BindingList<ColumnSampleDataTemplateSelectionItem> _templatesSource = new();
    private DataTable _selectedTablePreviewSource = new();

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

    public BindingList<ColumnSampleDataTemplateSelectionItem> TemplatesSource
    {
        get => _templatesSource;
        private set => SetProperty(ref _templatesSource, value);
    }

    public DataTable SelectedTablePreviewSource
    {
        get => _selectedTablePreviewSource;
        private set => SetProperty(ref _selectedTablePreviewSource, value);
    }

    /// <summary>
    /// テーブル操作に必要なリポジトリと生成サービスを受け取ります。
    /// </summary>
    public ConnectionOperationViewModel(
        IDbTableInfoRepository dbTableInfoRepository,
        IDbTableSchemaRepository dbTableSchemaRepository,
        ITestDataGenerator testDataGenerator,
        IBoundaryTestDataGenerator boundaryTestDataGenerator,
        ITestDataOutputRepository testDataOutputRepository,
        ITestDataDirectInsertRepository testDataDirectInsertRepository,
        ISampleDataRepository sampleDataRepository,
        IColumnSampleDataTemplateRepository templateRepository,
        IForeignKeyRelationRepository foreignKeyRelationRepository,
        IForeignKeyTestDataApplier foreignKeyTestDataApplier)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
        _dbTableSchemaRepository = dbTableSchemaRepository;
        _testDataGenerator = testDataGenerator;
        _boundaryTestDataGenerator = boundaryTestDataGenerator;
        _testDataOutputRepository = testDataOutputRepository;
        _testDataDirectInsertRepository = testDataDirectInsertRepository;
        _sampleDataRepository = sampleDataRepository;
        _templateRepository = templateRepository;
        _foreignKeyRelationRepository = foreignKeyRelationRepository;
        _foreignKeyTestDataApplier = foreignKeyTestDataApplier;
    }

    /// <summary>
    /// 指定されたDB接続のテーブル一覧とサンプルデータ種別を読み込みます。
    /// </summary>
    public async Task Initialize(DbConnectionInfo connection)
    {
        _connection = connection;
        _foreignKeySettings = _foreignKeyRelationRepository.GetAll().ToList();

        var tables = await _dbTableInfoRepository.GetTablesAsync(connection);

        TablesSource = new BindingList<DbTableSelectionItem>(
            tables.Select(table => new DbTableSelectionItem(table)).ToList());

        SampleDataKindsSource.Clear();
        SampleDataKindsSource.Add(string.Empty);

        foreach (var kind in _sampleDataRepository.GetKinds())
        {
            SampleDataKindsSource.Add(kind);
        }

        TemplatesSource.Clear();
    }

    /// <summary>
    /// 現在操作中のDB接続情報を返します。
    /// </summary>
    internal DbConnectionInfo GetCurrentConnection()
    {
        return _connection
            ?? throw new InvalidOperationException("DB接続情報が初期化されていません。");
    }

    /// <summary>
    /// 指定カラムに保存済みの外部キー設定を取得します。
    /// </summary>
    internal IReadOnlyList<ForeignKeyRelationSetting> GetForeignKeySettings(
        DbColumnSampleDataSelectionItem columnItem)
    {
        var column = columnItem.Column;

        return _foreignKeySettings
            .Where(setting =>
                setting.SourceSchemaName == column.SchemaName
                && setting.SourceTableName == column.TableName
                && setting.SourceColumnName == column.ColumnName)
            .ToList();
    }

    /// <summary>
    /// 指定カラムの外部キー設定を保存し、ViewModel内の保持情報も更新します。
    /// </summary>
    internal async Task SaveForeignKeySettings(
        DbColumnSampleDataSelectionItem columnItem,
        IReadOnlyList<ForeignKeyRelationSetting> settings)
    {
        var column = columnItem.Column;
        var currentColumnSettings = _foreignKeySettings
            .Where(setting => IsSourceColumn(setting, column))
            .ToList();
        var replacementSettings = settings
            .SelectMany(setting => new[]
            {
                setting,
                CreateReverseSetting(setting)
            });

        _foreignKeySettings = _foreignKeySettings
            .Where(setting => !IsSourceColumn(setting, column))
            .Where(setting => !currentColumnSettings.Any(current => IsSameRelation(setting, CreateReverseSetting(current))))
            .Concat(replacementSettings)
            .DistinctBy(CreateRelationKey)
            .ToList();

        await ApplySampleDataKindToReferenceColumns(columnItem, settings);
        await _foreignKeyRelationRepository.SaveAllAsync(_foreignKeySettings);
        RefreshForeignKeyDisplay(columnItem);
        ColumnsSource.ResetBindings();
    }

    /// <summary>
    /// 選択されたテーブルのカラム一覧を読み込み、テンプレート候補も更新します。
    /// </summary>
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

        RefreshForeignKeyDisplays(columnsSource);
        ColumnsSource = columnsSource;
        LoadTemplates(tableItem.Table);
    }

    /// <summary>
    /// 選択されたテーブルの実データプレビューを読み込みます。
    /// </summary>
    public async Task LoadSelectedTablePreview(DbTableSelectionItem? tableItem)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        if (tableItem == null)
        {
            SelectedTablePreviewSource = new DataTable();
            return;
        }

        SelectedTablePreviewSource = await _dbTableInfoRepository.GetPreviewDataAsync(
            _connection,
            tableItem.Table);
    }

    /// <summary>
    /// 現在表示中のカラム設定をテンプレートとして保存します。
    /// </summary>
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
        LoadTemplates(_currentTable);
    }

    /// <summary>
    /// 選択されたテンプレートの設定を現在のカラム一覧へ反映します。
    /// </summary>
    public void ApplyTemplate(ColumnSampleDataTemplateSelectionItem? templateItem)
    {
        if (templateItem == null)
        {
            return;
        }

        var template = templateItem.Template;

        foreach (var columnItem in ColumnsSource)
        {
            var templateColumn = template.Columns.FirstOrDefault(column =>
                column.ColumnName == columnItem.ColumnName);

            if (templateColumn == null)
            {
                continue;
            }

            columnItem.SampleDataKind = templateColumn.SampleDataKind;
        }

        ColumnsSource.ResetBindings();
    }

    /// <summary>
    /// 選択されたテーブルに対して指定件数の通常テストデータを生成します。
    /// </summary>
    public async Task<TestDataOutputResult> CreateTestData(int rowCount, bool directInsert = false)
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

        var appliedTestDataList = _foreignKeyTestDataApplier.Apply(testDataList, _foreignKeySettings);

        return directInsert
            ? await _testDataDirectInsertRepository.SaveAsync(_connection, appliedTestDataList)
            : await _testDataOutputRepository.SaveAsync(appliedTestDataList);
    }

    /// <summary>
    /// 選択されたテーブルに対して境界値テストデータを生成します。
    /// </summary>
    public async Task<TestDataOutputResult> CreateBoundaryTestData(bool directInsert = false)
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

        var appliedTestDataList = _foreignKeyTestDataApplier.Apply(testDataList, _foreignKeySettings);

        return directInsert
            ? await _testDataDirectInsertRepository.SaveAsync(_connection, appliedTestDataList)
            : await _testDataOutputRepository.SaveAsync(appliedTestDataList);
    }

    /// <summary>
    /// 作成対象としてチェックされたテーブル一覧を取得します。
    /// </summary>
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

    /// <summary>
    /// 指定テーブルに対して画面で設定されたサンプルデータ設定を取得します。
    /// </summary>
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

    /// <summary>
    /// テーブル単位のカラム設定キャッシュに使うキーを作成します。
    /// </summary>
    private static string CreateTableKey(DbTableInfo table)
    {
        return $"{table.SchemaName}.{table.TableName}";
    }

    /// <summary>
    /// テーブル単位のカラム設定キャッシュに使うキーを作成します。
    /// </summary>
    private static string CreateTableKey(string schemaName, string tableName)
    {
        return $"{schemaName}.{tableName}";
    }

    /// <summary>
    /// 操作中カラムの種類設定を、今回追加された外部キー参照先カラムへ反映します。
    /// </summary>
    private async Task ApplySampleDataKindToReferenceColumns(
        DbColumnSampleDataSelectionItem columnItem,
        IReadOnlyList<ForeignKeyRelationSetting> settings)
    {
        if (_connection == null || string.IsNullOrWhiteSpace(columnItem.SampleDataKind))
        {
            return;
        }

        foreach (var setting in settings)
        {
            var referenceColumns = await GetOrLoadColumnsSource(
                setting.ReferenceSchemaName,
                setting.ReferenceTableName);
            var referenceColumn = referenceColumns.FirstOrDefault(column =>
                column.Column.SchemaName == setting.ReferenceSchemaName
                && column.Column.TableName == setting.ReferenceTableName
                && column.Column.ColumnName == setting.ReferenceColumnName);

            if (referenceColumn == null)
            {
                continue;
            }

            referenceColumn.SampleDataKind = columnItem.SampleDataKind;
        }
    }

    /// <summary>
    /// 指定テーブルのカラム設定をキャッシュから取得し、未読込ならDBスキーマから読み込みます。
    /// </summary>
    private async Task<BindingList<DbColumnSampleDataSelectionItem>> GetOrLoadColumnsSource(
        string schemaName,
        string tableName)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        var key = CreateTableKey(schemaName, tableName);
        if (_columnsByTable.TryGetValue(key, out var columnsSource))
        {
            return columnsSource;
        }

        var table = new DbTableInfo
        {
            SchemaName = schemaName,
            TableName = tableName
        };
        var columns = await _dbTableSchemaRepository.GetColumnsAsync(_connection, table);
        columnsSource = new BindingList<DbColumnSampleDataSelectionItem>(
            columns.Select(column => new DbColumnSampleDataSelectionItem(column)).ToList());

        _columnsByTable.Add(key, columnsSource);

        return columnsSource;
    }

    /// <summary>
    /// 外部キー設定の参照元が指定カラムかどうかを判定します。
    /// </summary>
    private static bool IsSourceColumn(
        ForeignKeyRelationSetting setting,
        DbColumnInfo column)
    {
        return setting.SourceSchemaName == column.SchemaName
            && setting.SourceTableName == column.TableName
            && setting.SourceColumnName == column.ColumnName;
    }

    /// <summary>
    /// 外部キー設定の向きを反転した設定を作成します。
    /// </summary>
    private static ForeignKeyRelationSetting CreateReverseSetting(ForeignKeyRelationSetting setting)
    {
        return new ForeignKeyRelationSetting
        {
            SourceSchemaName = setting.ReferenceSchemaName,
            SourceTableName = setting.ReferenceTableName,
            SourceColumnName = setting.ReferenceColumnName,
            ReferenceSchemaName = setting.SourceSchemaName,
            ReferenceTableName = setting.SourceTableName,
            ReferenceColumnName = setting.SourceColumnName
        };
    }

    /// <summary>
    /// 2つの外部キー設定が同じ向きの同じ関係かどうかを判定します。
    /// </summary>
    private static bool IsSameRelation(
        ForeignKeyRelationSetting left,
        ForeignKeyRelationSetting right)
    {
        return CreateRelationKey(left) == CreateRelationKey(right);
    }

    /// <summary>
    /// 外部キー設定の重複判定に使うキーを作成します。
    /// </summary>
    private static string CreateRelationKey(ForeignKeyRelationSetting setting)
    {
        return string.Join(
            "|",
            setting.SourceSchemaName,
            setting.SourceTableName,
            setting.SourceColumnName,
            setting.ReferenceSchemaName,
            setting.ReferenceTableName,
            setting.ReferenceColumnName);
    }

    /// <summary>
    /// 選択中テーブルに対応するテンプレート一覧を読み込みます。
    /// </summary>
    private void LoadTemplates(DbTableInfo? table)
    {
        if (table == null)
        {
            TemplatesSource = new BindingList<ColumnSampleDataTemplateSelectionItem>();
            return;
        }

        TemplatesSource = new BindingList<ColumnSampleDataTemplateSelectionItem>(
            _templateRepository
                .GetAll()
                .Where(template =>
                    template.SchemaName == table.SchemaName
                    && template.TableName == table.TableName)
                .Select(template => new ColumnSampleDataTemplateSelectionItem(template))
                .ToList());
    }

    /// <summary>
    /// 表示中カラム一覧の外部キー参照先表示を更新します。
    /// </summary>
    private void RefreshForeignKeyDisplays(
        BindingList<DbColumnSampleDataSelectionItem> columnsSource)
    {
        foreach (var columnItem in columnsSource)
        {
            RefreshForeignKeyDisplay(columnItem);
        }
    }

    /// <summary>
    /// 指定カラムに設定された外部キー参照先を表示用文字列に反映します。
    /// </summary>
    private void RefreshForeignKeyDisplay(DbColumnSampleDataSelectionItem columnItem)
    {
        columnItem.ForeignKeyDisplay = string.Join(
            ", ",
            GetForeignKeySettings(columnItem)
                .Select(setting => $"{setting.ReferenceSchemaName}.{setting.ReferenceTableName}.{setting.ReferenceColumnName}"));
    }
}
