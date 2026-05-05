using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.ViewModels;
using System.Threading.Tasks;

namespace SampleDataMaker.WinForm.Views;

public partial class MainView : Form
{
    private MainViewModel _vm;

    public MainView()
    {
        InitializeComponent();

        _vm = DI.Resolve<MainViewModel>();
        SetupDgvConnections();
        dgvConnections.DataBindings.Add(nameof(dgvConnections.DataSource), _vm, nameof(_vm.DgvConnectionsSource));
        this.Load += async (_, __) => await _vm.MainViewLoad();
        this.RegisterButton.Click += async (_, __) => await _vm.Save();
        this.AddOperationButtonColumn();
        this.dgvConnections.CellContentClick += async (_, __) => await DgvConnections_CellContentClick(_, __);
    }

    private void SetupDgvConnections()
    {
        dgvConnections.AutoGenerateColumns = false;
        dgvConnections.Columns.Clear();

        dgvConnections.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DbType",
            HeaderText = "DbType",
            DataPropertyName = nameof(DbConnectionInfo.DbType),
            Width = 90
        });

        dgvConnections.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ConnectionString",
            HeaderText = "ConnectionString",
            DataPropertyName = nameof(DbConnectionInfo.ConnectionString),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void AddOperationButtonColumn()
    {
        var titleLinkColumn = new DataGridViewLinkColumn
        {
            Name = "Title",
            HeaderText = "Title",
            DataPropertyName = nameof(DbConnectionInfo.Title),
            TrackVisitedState = false,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Width = 180
        };

        dgvConnections.Columns.Insert(0, titleLinkColumn);
    }

    private async Task DgvConnections_CellContentClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var columnName = dgvConnections.Columns[e.ColumnIndex].Name;
        var connection = dgvConnections.Rows[e.RowIndex].DataBoundItem as DbConnectionInfo;

        await _vm.DgvConnectionsCellContentClick(columnName, connection);
    }
}
