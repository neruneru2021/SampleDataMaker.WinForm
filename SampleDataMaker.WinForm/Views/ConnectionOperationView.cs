using SampleDataMaker.WinForm.ViewModels;
using System.Diagnostics;

namespace SampleDataMaker.WinForm.Views;

public partial class ConnectionOperationView : Form
{
    private readonly ConnectionOperationViewModel _vm;

    internal ConnectionOperationView(ConnectionOperationViewModel vm)
    {
        InitializeComponent();

        _vm = vm;

        SetupDgvTables();
        SetupColumnsDataGridView();

        dgvTables.DataBindings.Add(
            nameof(dgvTables.DataSource),
            _vm,
            nameof(_vm.TablesSource));

        ColumnsDataGridView.DataBindings.Add(
            nameof(ColumnsDataGridView.DataSource),
            _vm,
            nameof(_vm.ColumnsSource));
    }

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

    private void SetupColumnsDataGridView()
    {
        ColumnsDataGridView.AutoGenerateColumns = false;
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

        ColumnsDataGridView.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "UseSampleData",
            HeaderText = "サンプル使用",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.UseSampleData),
            Width = 100
        });

        ColumnsDataGridView.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "SampleDataKind",
            HeaderText = "種類",
            DataPropertyName = nameof(DbColumnSampleDataSelectionItem.SampleDataKind),
            DataSource = _vm.SampleDataKindsSource,
            Width = 140
        });

        ColumnsDataGridView.DataError += (_, __) => { };
    }

    private async Task DgvTablesCellClick(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        ColumnsDataGridView.EndEdit();

        var tableItem = dgvTables.Rows[e.RowIndex].DataBoundItem as DbTableSelectionItem;

        await _vm.LoadColumns(tableItem);
    }

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

    private async Task TemplateButtonClick()
    {
        try
        {
            ColumnsDataGridView.EndEdit();

            await _vm.SaveCurrentTemplate(TemplateNameTextBox.Text);

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

    private void CreateCountTextBoxKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
        {
            return;
        }

        e.Handled = true;
    }

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

    private int GetCreateCount()
    {
        if (!int.TryParse(CreateCountTextBox.Text, out var createCount) || createCount <= 0)
        {
            throw new InvalidOperationException("作成件数は1以上の数値を入力してください。");
        }

        return createCount;
    }
}
