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

        dgvTables.DataBindings.Add(
            nameof(dgvTables.DataSource),
            _vm,
            nameof(_vm.TablesSource));
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
    }

    private async Task CreateButtonClick()
    {
        try
        {
            dgvTables.EndEdit();

            var result = await _vm.CreateTestData();

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
}
