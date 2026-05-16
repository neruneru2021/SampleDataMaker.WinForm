using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.Services;
using SampleDataMaker.WinForm.ViewModels;

namespace SampleDataMaker.WinForm.Views
{
    /// <summary>
    /// 外部キーの参照先テーブルとカラムを選択する画面です。
    /// </summary>
    public partial class ForeignKeySelectView : Form
    {
        private readonly ForeignKeySelectViewModel _vm;

        /// <summary>
        /// 外部キー選択画面を初期化し、一覧のバインドとイベントを設定します。
        /// </summary>
        internal ForeignKeySelectView(ForeignKeySelectViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            SetupForeignKeyTableDataGridView();
            SetupForeignKeyColumnDataGridView();
            SetupForeignKeyDataGridView();
            SetupSelectFKeyDataGridView();

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

            SelectFKeyDataGridView.DataBindings.Add(
                nameof(SelectFKeyDataGridView.DataSource),
                _vm,
                nameof(_vm.SelectedTablePreviewSource));

            ForeignKeyTableDataGridView.CellClick += async (_, e) => await ForeignKeyTableDataGridViewCellClick(e);
            ForeignKeyTableDataGridView.CellFormatting += (_, e) => ForeignKeyTableDataGridViewCellFormatting(e);
            ForeignKeyColumnDataGridView.CellDoubleClick += (_, e) => ForeignKeyColumnDataGridViewCellDoubleClick(e);
            ForeignKeyDataGridView.CellDoubleClick += (_, e) => ForeignKeyDataGridViewCellDoubleClick(e);
            ConfirmedButton.Click += (_, __) => ConfirmedButtonClick();
        }

        public IReadOnlyList<SampleDataMaker.Domain.Entities.ForeignKeyRelationSetting> ConfirmedSettings { get; private set; }
            = Array.Empty<SampleDataMaker.Domain.Entities.ForeignKeyRelationSetting>();

        /// <summary>
        /// 接続先と外部キー設定元カラムが分かるように画面タイトルを設定します。
        /// </summary>
        internal void SetForeignKeyTitle(
            DbConnectionInfo connection,
            DbColumnInfo sourceColumn)
        {
            Text = ConnectionTitleFormatter.CreateForeignKeyTitle(connection, sourceColumn);
        }

        /// <summary>
        /// 参照先候補テーブル一覧グリッドの表示列を設定します。
        /// </summary>
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

        /// <summary>
        /// 選択テーブルの実データプレビュー用グリッドを設定します。
        /// </summary>
        private void SetupSelectFKeyDataGridView()
        {
            SelectFKeyDataGridView.AutoGenerateColumns = true;
            SelectFKeyDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            SelectFKeyDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            SelectFKeyDataGridView.AllowUserToAddRows = false;
            SelectFKeyDataGridView.AllowUserToDeleteRows = false;
            SelectFKeyDataGridView.ReadOnly = true;
        }

        /// <summary>
        /// 参照先候補カラム一覧グリッドの表示列を設定します。
        /// </summary>
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

        /// <summary>
        /// 選択済み外部キー一覧グリッドの表示列を設定します。
        /// </summary>
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

        /// <summary>
        /// テーブルクリック時にカラム一覧と実データプレビューを切り替えます。
        /// </summary>
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
                await _vm.LoadSelectedTablePreview(tableItem);
                return;
            }

            await _vm.LoadColumns(tableItem);
            await _vm.LoadSelectedTablePreview(tableItem);
        }

        /// <summary>
        /// 選択対象にできないテーブルをグレー表示にします。
        /// </summary>
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

        /// <summary>
        /// カラムのダブルクリックで参照先外部キー候補に追加します。
        /// </summary>
        private void ForeignKeyColumnDataGridViewCellDoubleClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var columnItem = ForeignKeyColumnDataGridView.Rows[e.RowIndex].DataBoundItem as DbColumnSampleDataSelectionItem;

            _vm.AddForeignKey(columnItem);
        }

        /// <summary>
        /// 選択済み外部キーのダブルクリックで候補から削除します。
        /// </summary>
        private void ForeignKeyDataGridViewCellDoubleClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            _vm.RemoveForeignKeyAt(e.RowIndex);
        }

        /// <summary>
        /// 現在の外部キー選択内容を確定して画面を閉じます。
        /// </summary>
        private void ConfirmedButtonClick()
        {
            ConfirmedSettings = _vm.Confirm();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
