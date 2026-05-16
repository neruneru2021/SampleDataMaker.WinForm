using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.Services;
using SampleDataMaker.WinForm.ViewModels;
using System.Diagnostics;

namespace SampleDataMaker.WinForm.Views;

/// <summary>
/// 選択したDB接続に対するテーブル選択、カラム設定、データプレビュー、テストデータ作成を行う画面です。
/// </summary>
public partial class ConnectionOperationView : Form
{
    private readonly ConnectionOperationViewModel _vm;

    /// <summary>
    /// テーブル操作画面を初期化し、各グリッドと入力イベントを設定します。
    /// </summary>
    internal ConnectionOperationView(ConnectionOperationViewModel vm)
    {
        InitializeComponent();

        _vm = vm;

        SetupDgvTables();
        SetupColumnsDataGridView();
        SetupSelectTableDataGridView();

        dgvTables.DataBindings.Add(
            nameof(dgvTables.DataSource),
            _vm,
            nameof(_vm.TablesSource));

        ColumnsDataGridView.DataBindings.Add(
            nameof(ColumnsDataGridView.DataSource),
            _vm,
            nameof(_vm.ColumnsSource));

        SelectTableDataGridView.DataBindings.Add(
            nameof(SelectTableDataGridView.DataSource),
            _vm,
            nameof(_vm.SelectedTablePreviewSource));

        TemplateComboBox.DataSource = _vm.TemplatesSource;
        TemplateComboBox.DisplayMember = nameof(ColumnSampleDataTemplateSelectionItem.DisplayName);
        TemplateComboBox.SelectedIndex = -1;
        TemplateComboBox.SelectedIndexChanged += TemplateComboBoxSelectedIndexChanged;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(_vm.TemplatesSource))
            {
                return;
            }

            RefreshTemplateComboBox();
        };
    }

    /// <summary>
    /// 開いているDB接続が分かるように画面タイトルを設定します。
    /// </summary>
    internal void SetConnectionTitle(DbConnectionInfo connection)
    {
        Text = ConnectionTitleFormatter.CreateOperationTitle(connection);
    }

    /// <summary>
    /// テーブル一覧グリッドの表示列と操作イベントを設定します。
    /// </summary>
    private void SetupDgvTables()
    {
        dgvTables.AutoGenerateColumns = false;
        dgvTables.Columns.Clear();

        dgvTables.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "IsSelected",
            HeaderText = "",
            DataPropertyName = nameof(DbTableSelectionItem.IsSelected),
            Width = 40
        });

        dgvTables.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SchemaName",
            HeaderText = "Schema",
            DataPropertyName = nameof(DbTableSelectionItem.SchemaName),
            Width = 120
        });

        dgvTables.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TableName",
            HeaderText = "Table",
            DataPropertyName = nameof(DbTableSelectionItem.TableName),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        CreateButton.Click += async (_, __) => await CreateButtonClick();
        Create2Button.Click += async (_, __) => await Create2ButtonClick();
        TemplateButton.Click += async (_, __) => await TemplateButtonClick();
        dgvTables.CellClick += async (_, e) => await DgvTablesCellClick(e);
        dgvTables.CurrentCellDirtyStateChanged += (_, __) => DgvTablesCurrentCellDirtyStateChanged();
        CreateCountTextBox.KeyPress += CreateCountTextBoxKeyPress;
        CreateCountTextBox.TextChanged += (_, __) => SanitizeCreateCountTextBox();

        if (string.IsNullOrWhiteSpace(CreateCountTextBox.Text))
        {
            CreateCountTextBox.Text = "1";
        }
    }

    /// <summary>
    /// カラム一覧グリッドの表示列と外部キー設定ボタンを設定します。
    /// </summary>
    private void SetupColumnsDataGridView()
    {
        ColumnsDataGridView.AutoGenerateColumns = false;
        ColumnsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        ColumnsDataGridView.Columns.Clear();

        ColumnsDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColumnName",
            HeaderText = "カラム名",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.ColumnName),
            ReadOnly = true,
            Width = 160
        });

        ColumnsDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DataType",
            HeaderText = "データ型",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.DataType),
            ReadOnly = true,
            Width = 120
        });

        ColumnsDataGridView.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "SampleDataKind",
            HeaderText = "種類",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.SampleDataKind),
            DataSource = _vm.SampleDataKindsSource,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            MinimumWidth = 100,
            Width = 110
        });

        ColumnsDataGridView.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "ForeignKey",
            HeaderText = "外部キー",
            Text = "設定",
            UseColumnTextForButtonValue = true,
            Width = 80
        });

        ColumnsDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ForeignKeyDisplay",
            HeaderText = "設定済み外部キー",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.ForeignKeyDisplay),
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 180
        });

        ColumnsDataGridView.DataError += (_, __) => { };
        ColumnsDataGridView.CellClick += ColumnsDataGridViewCellClick;
        ColumnsDataGridView.CellContentClick += async (_, e) => await ColumnsDataGridViewCellContentClick(e);
    }

    /// <summary>
    /// 選択テーブルの実データプレビュー用グリッドを設定します。
    /// </summary>
    private void SetupSelectTableDataGridView()
    {
        SelectTableDataGridView.AutoGenerateColumns = true;
        SelectTableDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        SelectTableDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        SelectTableDataGridView.AllowUserToAddRows = false;
        SelectTableDataGridView.AllowUserToDeleteRows = false;
        SelectTableDataGridView.ReadOnly = true;
    }

    /// <summary>
    /// 種類セルのクリック時にコンボボックスを開きます。
    /// </summary>
    private void ColumnsDataGridViewCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (ColumnsDataGridView.Columns[e.ColumnIndex].Name != "SampleDataKind")
        {
            return;
        }

        ColumnsDataGridView.BeginEdit(true);
        BeginInvoke(() =>
        {
            if (ColumnsDataGridView.EditingControl is ComboBox comboBox)
            {
                comboBox.DroppedDown = true;
            }
        });
    }

    /// <summary>
    /// 外部キー設定ボタンのクリックで参照先選択画面を開きます。
    /// </summary>
    private async Task ColumnsDataGridViewCellContentClick(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (ColumnsDataGridView.Columns[e.ColumnIndex].Name != "ForeignKey")
        {
            return;
        }

        ColumnsDataGridView.EndEdit();

        if (ColumnsDataGridView.Rows[e.RowIndex].DataBoundItem is not DbColumnSampleDataSelectionItem columnItem)
        {
            return;
        }

        var foreignKeySelectViewModel = DI.Resolve<ForeignKeySelectViewModel>();
        await foreignKeySelectViewModel.Initialize(
            _vm.GetCurrentConnection(),
            columnItem.Column,
            _vm.GetForeignKeySettings(columnItem));

        using var view = new ForeignKeySelectView(foreignKeySelectViewModel);
        view.SetForeignKeyTitle(_vm.GetCurrentConnection(), columnItem.Column);

        if (view.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await _vm.SaveForeignKeySettings(columnItem, view.ConfirmedSettings);
    }

    /// <summary>
    /// テーブルクリック時にカラム一覧、実データプレビュー、テンプレート候補を切り替えます。
    /// </summary>
    private async Task DgvTablesCellClick(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        ColumnsDataGridView.EndEdit();

        var tableItem = dgvTables.Rows[e.RowIndex].DataBoundItem as DbTableSelectionItem;

        await _vm.LoadColumns(tableItem);
        await _vm.LoadSelectedTablePreview(tableItem);
        RefreshTemplateComboBox();
    }

    /// <summary>
    /// チェックボックス列の変更を即時コミットします。
    /// </summary>
    private void DgvTablesCurrentCellDirtyStateChanged()
    {
        if (!dgvTables.IsCurrentCellDirty)
        {
            return;
        }

        if (dgvTables.CurrentCell is DataGridViewCheckBoxCell)
        {
            dgvTables.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    /// <summary>
    /// 選択テーブルに対して指定件数の通常テストデータを作成します。
    /// </summary>
    private async Task CreateButtonClick()
    {
        try
        {
            dgvTables.EndEdit();
            ColumnsDataGridView.EndEdit();

            var result = await _vm.CreateTestData(GetCreateCount());

            MessageBox.Show(
                $"テストデータを作成しました。\r\n\r\n保存先: {result.OutputDirectoryPath}",
                "作成完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Process.Start(new ProcessStartInfo
            {
                FileName = result.OutputDirectoryPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"テストデータの作成に失敗しました。\r\n\r\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 選択テーブルに対して境界値テストデータを作成します。
    /// </summary>
    private async Task Create2ButtonClick()
    {
        try
        {
            dgvTables.EndEdit();
            ColumnsDataGridView.EndEdit();

            var result = await _vm.CreateBoundaryTestData();

            MessageBox.Show(
                $"種類別テストデータを作成しました。\r\n\r\n保存先: {result.OutputDirectoryPath}",
                "作成完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Process.Start(new ProcessStartInfo
            {
                FileName = result.OutputDirectoryPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"種類別テストデータの作成に失敗しました。\r\n\r\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 現在のカラムサンプル設定をテンプレートとして保存します。
    /// </summary>
    private async Task TemplateButtonClick()
    {
        try
        {
            ColumnsDataGridView.EndEdit();

            await _vm.SaveCurrentTemplate(TemplateNameTextBox.Text);
            RefreshTemplateComboBox();

            MessageBox.Show(
                "テンプレートを保存しました。",
                "保存完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"テンプレートの保存に失敗しました。\r\n\r\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 選択されたテンプレートを現在のカラム設定へ適用します。
    /// </summary>
    private void TemplateComboBoxSelectedIndexChanged(object? sender, EventArgs e)
    {
        ColumnsDataGridView.EndEdit();

        _vm.ApplyTemplate(TemplateComboBox.SelectedItem as ColumnSampleDataTemplateSelectionItem);
    }

    /// <summary>
    /// テンプレート一覧の再バインド時に選択イベントの重複発火を防ぎます。
    /// </summary>
    private void RefreshTemplateComboBox()
    {
        TemplateComboBox.SelectedIndexChanged -= TemplateComboBoxSelectedIndexChanged;
        TemplateComboBox.DataSource = _vm.TemplatesSource;
        TemplateComboBox.DisplayMember = nameof(ColumnSampleDataTemplateSelectionItem.DisplayName);
        TemplateComboBox.SelectedIndex = -1;
        TemplateComboBox.SelectedIndexChanged += TemplateComboBoxSelectedIndexChanged;
    }

    /// <summary>
    /// 作成件数入力を数字と制御キーだけに制限します。
    /// </summary>
    private void CreateCountTextBoxKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
        {
            return;
        }

        e.Handled = true;
    }

    /// <summary>
    /// 貼り付けなどで混入した数字以外の文字を作成件数から取り除きます。
    /// </summary>
    private void SanitizeCreateCountTextBox()
    {
        var text = CreateCountTextBox.Text;
        var numericText = new string(text.Where(char.IsDigit).ToArray());

        if (text == numericText)
        {
            return;
        }

        var selectionStart = Math.Min(CreateCountTextBox.SelectionStart, numericText.Length);
        CreateCountTextBox.Text = numericText;
        CreateCountTextBox.SelectionStart = selectionStart;
    }

    /// <summary>
    /// 作成件数の入力値を検証して数値として返します。
    /// </summary>
    private int GetCreateCount()
    {
        if (!int.TryParse(CreateCountTextBox.Text, out var createCount) || createCount <= 0)
        {
            throw new InvalidOperationException("作成件数は1以上の数値を入力してください。");
        }

        return createCount;
    }

}
