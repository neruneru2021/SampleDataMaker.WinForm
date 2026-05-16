using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.WinForm.Services;
using System.ComponentModel;
using System.Data;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// 外部キー参照先選択画面のテーブル、カラム、選択済み外部キーを管理します。
/// </summary>
internal class ForeignKeySelectViewModel : ViewModelBase
{
    private readonly IDbTableInfoRepository _dbTableInfoRepository;
    private readonly IDbTableSchemaRepository _dbTableSchemaRepository;
    private readonly IForeignKeyTypeMismatchConfirmationService _typeMismatchConfirmationService;
    private DbConnectionInfo? _connection;
    private DbColumnInfo? _sourceColumn;

    private BindingList<DbTableSelectionItem> _tablesSource = new();
    private BindingList<DbColumnSampleDataSelectionItem> _columnsSource = new();
    private BindingList<ForeignKeyRelationSelectionItem> _foreignKeySource = new();
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

    public BindingList<ForeignKeyRelationSelectionItem> ForeignKeySource
    {
        get => _foreignKeySource;
        private set => SetProperty(ref _foreignKeySource, value);
    }

    public DataTable SelectedTablePreviewSource
    {
        get => _selectedTablePreviewSource;
        private set => SetProperty(ref _selectedTablePreviewSource, value);
    }

    /// <summary>
    /// テーブル情報とカラム情報の取得リポジトリを受け取ります。
    /// </summary>
    public ForeignKeySelectViewModel(
        IDbTableInfoRepository dbTableInfoRepository,
        IDbTableSchemaRepository dbTableSchemaRepository,
        IForeignKeyTypeMismatchConfirmationService typeMismatchConfirmationService)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
        _dbTableSchemaRepository = dbTableSchemaRepository;
        _typeMismatchConfirmationService = typeMismatchConfirmationService;
    }

    /// <summary>
    /// 参照先候補テーブルと現在の外部キー設定を読み込みます。
    /// </summary>
    public async Task Initialize(
        DbConnectionInfo connection,
        DbColumnInfo sourceColumn,
        IReadOnlyList<ForeignKeyRelationSetting> currentSettings)
    {
        _connection = connection;
        _sourceColumn = sourceColumn;

        var tables = await _dbTableInfoRepository.GetTablesAsync(connection);
        TablesSource = new BindingList<DbTableSelectionItem>(
            tables.Select(table => new DbTableSelectionItem(table)
            {
                IsEnabled = !IsSourceTable(table, sourceColumn)
            }).ToList());

        ForeignKeySource = new BindingList<ForeignKeyRelationSelectionItem>(
            currentSettings.Select(setting => new ForeignKeyRelationSelectionItem
            {
                SchemaName = setting.ReferenceSchemaName,
                TableName = setting.ReferenceTableName,
                ColumnName = setting.ReferenceColumnName
            }).ToList());
    }

    /// <summary>
    /// 選択された参照先テーブルのカラム一覧を読み込みます。
    /// </summary>
    public async Task LoadColumns(DbTableSelectionItem? tableItem)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        if (tableItem == null || !tableItem.IsEnabled)
        {
            return;
        }

        var columns = await _dbTableSchemaRepository.GetColumnsAsync(_connection, tableItem.Table);
        ColumnsSource = new BindingList<DbColumnSampleDataSelectionItem>(
            columns.Select(column => new DbColumnSampleDataSelectionItem(column)).ToList());
    }

    /// <summary>
    /// 選択された参照先テーブルの実データプレビューを読み込みます。
    /// </summary>
    public async Task LoadSelectedTablePreview(DbTableSelectionItem? tableItem)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB接続情報が初期化されていません。");
        }

        if (tableItem == null || !tableItem.IsEnabled)
        {
            SelectedTablePreviewSource = new DataTable();
            return;
        }

        SelectedTablePreviewSource = await _dbTableInfoRepository.GetPreviewDataAsync(
            _connection,
            tableItem.Table);
    }

    /// <summary>
    /// ダブルクリックされたカラムを外部キー参照先として追加します。
    /// </summary>
    public void AddForeignKey(DbColumnSampleDataSelectionItem? columnItem)
    {
        if (columnItem == null || _sourceColumn == null)
        {
            return;
        }

        var column = columnItem.Column;

        if (!IsSameDataType(_sourceColumn, column)
            && !_typeMismatchConfirmationService.Confirm(_sourceColumn, column))
        {
            return;
        }

        var exists = ForeignKeySource.Any(item =>
            item.SchemaName == column.SchemaName
            && item.TableName == column.TableName
            && item.ColumnName == column.ColumnName);

        if (exists)
        {
            return;
        }

        ForeignKeySource.Add(new ForeignKeyRelationSelectionItem
        {
            SchemaName = column.SchemaName,
            TableName = column.TableName,
            ColumnName = column.ColumnName
        });
    }

    /// <summary>
    /// 指定された行の外部キー参照先を選択済み一覧から削除します。
    /// </summary>
    public void RemoveForeignKeyAt(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= ForeignKeySource.Count)
        {
            return;
        }

        ForeignKeySource.RemoveAt(rowIndex);
    }

    /// <summary>
    /// 画面で選択された参照先を保存可能な外部キー設定として確定します。
    /// </summary>
    public IReadOnlyList<ForeignKeyRelationSetting> Confirm()
    {
        if (_sourceColumn == null)
        {
            throw new InvalidOperationException("外部キー設定元のカラムが初期化されていません。");
        }

        return ForeignKeySource
            .Select(item => item.ToSetting(_sourceColumn))
            .ToList();
    }

    /// <summary>
    /// 外部キー設定元と同じテーブルかどうかを判定します。
    /// </summary>
    private static bool IsSourceTable(
        DbTableInfo table,
        DbColumnInfo sourceColumn)
    {
        return table.SchemaName == sourceColumn.SchemaName
            && table.TableName == sourceColumn.TableName;
    }

    /// <summary>
    /// 外部キー設定元と参照先のデータ型が同じかどうかを判定します。
    /// </summary>
    private static bool IsSameDataType(
        DbColumnInfo sourceColumn,
        DbColumnInfo referenceColumn)
    {
        return NormalizeDataType(sourceColumn.DataType)
            .Equals(NormalizeDataType(referenceColumn.DataType), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// NUMBER(10)のような桁指定を除外し、型名だけで比較できる形に整えます。
    /// </summary>
    private static string NormalizeDataType(string dataType)
    {
        var normalized = dataType.Trim();
        var parenthesisIndex = normalized.IndexOf('(');

        return parenthesisIndex < 0
            ? normalized
            : normalized[..parenthesisIndex].Trim();
    }
}
