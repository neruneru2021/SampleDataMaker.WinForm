using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.ViewModels;
using System.Threading.Tasks;

namespace SampleDataMaker.WinForm.Views;

/// <summary>
/// 登録済みDB接続の一覧表示、保存、接続先選択を行うメイン画面です。
/// </summary>
public partial class MainView : Form
{
    private MainViewModel _vm;

    /// <summary>
    /// メイン画面を初期化し、接続一覧のバインドとイベントを設定します。
    /// </summary>
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

    /// <summary>
    /// 接続一覧グリッドの表示列を設定します。
    /// </summary>
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
            Name = "DefaultSchema",
            HeaderText = "DefaultSchema",
            DataPropertyName = nameof(DbConnectionInfo.DefaultSchema),
            Width = 120
        });

        dgvConnections.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ConnectionString",
            HeaderText = "ConnectionString",
            DataPropertyName = nameof(DbConnectionInfo.ConnectionString),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    /// <summary>
    /// 接続先を開くためのタイトルリンク列を追加します。
    /// </summary>
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

    /// <summary>
    /// タイトルリンクのクリックをViewModelへ渡します。
    /// </summary>
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
