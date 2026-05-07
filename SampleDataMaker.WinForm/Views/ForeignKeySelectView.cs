using SampleDataMaker.WinForm.ViewModels;

namespace SampleDataMaker.WinForm.Views
{
    public partial class ForeignKeySelectView : Form
    {
        private readonly ForeignKeySelectViewModel _vm;

        internal ForeignKeySelectView(ForeignKeySelectViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            SetupForeignKeyTableDataGridView();
            SetupForeignKeyColumnDataGridView();
            SetupForeignKeyDataGridView();

            ForeignKeyTableDataGridView.DataBindings.Add(
                nameof(ForeignKeyTableDataGridView.DataSource),
                _vm,
                nameof(_vm.TablesSource));

            ForeignKeyColumnDataGridView.DataBindings.Add(
                nameof(ForeignKeyColumnDataGridView.DataSource),
                _vm,
                nameof(_vm.ColumnsSource));

            ForeignKeyDataGridView.DataBindings.Add(
                nameof(ForeignKeyDataGridView.DataSource),
                _vm,
                nameof(_vm.ForeignKeySource));

            ForeignKeyTableDataGridView.CellClick += async (_, e) => await ForeignKeyTableDataGridViewCellClick(e);
            ForeignKeyTableDataGridView.CellFormatting += (_, e) => ForeignKeyTableDataGridViewCellFormatting(e);
            ForeignKeyColumnDataGridView.CellDoubleClick += (_, e) => ForeignKeyColumnDataGridViewCellDoubleClick(e);
            ForeignKeyDataGridView.CellDoubleClick += (_, e) => ForeignKeyDataGridViewCellDoubleClick(e);
            ConfirmedButton.Click += (_, __) => ConfirmedButtonClick();
        }

        public IReadOnlyList<SampleDataMaker.Domain.Entities.ForeignKeyRelationSetting> ConfirmedSettings { get; private set; }
            = Array.Empty<SampleDataMaker.Domain.Entities.ForeignKeyRelationSetting>();

        private void SetupForeignKeyTableDataGridView()
        {
            ForeignKeyTableDataGridView.AutoGenerateColumns = false;
            ForeignKeyTableDataGridView.Columns.Clear();

            ForeignKeyTableDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SchemaName",
                HeaderText = "Schema",
                DataPropertyName = nameof(DbTableSelectionItem.SchemaName),
                Width = 100
            });

            ForeignKeyTableDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TableName",
                HeaderText = "Table",
                DataPropertyName = nameof(DbTableSelectionItem.TableName),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void SetupForeignKeyColumnDataGridView()
        {
            ForeignKeyColumnDataGridView.AutoGenerateColumns = false;
            ForeignKeyColumnDataGridView.Columns.Clear();

            ForeignKeyColumnDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColumnName",
                HeaderText = "カラム名",
                DataPropertyName = nameof(DbColumnSampleDataSelectionItem.ColumnName),
                Width = 160
            });

            ForeignKeyColumnDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DataType",
                HeaderText = "データ型",
                DataPropertyName = nameof(DbColumnSampleDataSelectionItem.DataType),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void SetupForeignKeyDataGridView()
        {
            ForeignKeyDataGridView.AutoGenerateColumns = false;
            ForeignKeyDataGridView.Columns.Clear();

            ForeignKeyDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TableName",
                HeaderText = "テーブル名",
                DataPropertyName = nameof(ForeignKeyRelationSelectionItem.TableName),
                Width = 120
            });

            ForeignKeyDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ColumnName",
                HeaderText = "カラム名",
                DataPropertyName = nameof(ForeignKeyRelationSelectionItem.ColumnName),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async Task ForeignKeyTableDataGridViewCellClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var tableItem = ForeignKeyTableDataGridView.Rows[e.RowIndex].DataBoundItem as DbTableSelectionItem;

            if (tableItem?.IsEnabled == false)
            {
                ForeignKeyColumnDataGridView.ClearSelection();
                return;
            }

            await _vm.LoadColumns(tableItem);
        }

        private void ForeignKeyTableDataGridViewCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (ForeignKeyTableDataGridView.Rows[e.RowIndex].DataBoundItem is not DbTableSelectionItem tableItem)
            {
                return;
            }

            if (tableItem.IsEnabled)
            {
                return;
            }

            var row = ForeignKeyTableDataGridView.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = Color.LightGray;
            row.DefaultCellStyle.ForeColor = Color.DarkGray;
            row.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            row.DefaultCellStyle.SelectionForeColor = Color.DarkGray;
            row.ReadOnly = true;
        }

        private void ForeignKeyColumnDataGridViewCellDoubleClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var columnItem = ForeignKeyColumnDataGridView.Rows[e.RowIndex].DataBoundItem as DbColumnSampleDataSelectionItem;

            _vm.AddForeignKey(columnItem);
        }

        private void ForeignKeyDataGridViewCellDoubleClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            _vm.RemoveForeignKeyAt(e.RowIndex);
        }

        private void ConfirmedButtonClick()
        {
            ConfirmedSettings = _vm.Confirm();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
