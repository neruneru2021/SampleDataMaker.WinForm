using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using System.ComponentModel;

namespace SampleDataMaker.WinForm.ViewModels;

internal class ForeignKeySelectViewModel : ViewModelBase
{
    private readonly IDbTableInfoRepository _dbTableInfoRepository;
    private readonly IDbTableSchemaRepository _dbTableSchemaRepository;
    private DbConnectionInfo? _connection;
    private DbColumnInfo? _sourceColumn;

    private BindingList<DbTableSelectionItem> _tablesSource = new();
    private BindingList<DbColumnSampleDataSelectionItem> _columnsSource = new();
    private BindingList<ForeignKeyRelationSelectionItem> _foreignKeySource = new();

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

    public ForeignKeySelectViewModel(
        IDbTableInfoRepository dbTableInfoRepository,
        IDbTableSchemaRepository dbTableSchemaRepository)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
        _dbTableSchemaRepository = dbTableSchemaRepository;
    }

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

    public void AddForeignKey(DbColumnSampleDataSelectionItem? columnItem)
    {
        if (columnItem == null)
        {
            return;
        }

        var column = columnItem.Column;
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

    public void RemoveForeignKeyAt(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= ForeignKeySource.Count)
        {
            return;
        }

        ForeignKeySource.RemoveAt(rowIndex);
    }

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

    private static bool IsSourceTable(
        DbTableInfo table,
        DbColumnInfo sourceColumn)
    {
        return table.SchemaName == sourceColumn.SchemaName
            && table.TableName == sourceColumn.TableName;
    }
}
