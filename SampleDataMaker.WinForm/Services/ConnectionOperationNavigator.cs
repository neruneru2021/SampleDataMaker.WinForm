using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.ViewModels;
using SampleDataMaker.WinForm.Views;

namespace SampleDataMaker.WinForm.Services;

/// <summary>
/// テーブル操作画面のViewModelを初期化し、画面表示を行います。
/// </summary>
public class ConnectionOperationNavigator : IConnectionOperationNavigator
{
    /// <summary>
    /// 指定されたDB接続でテーブル操作画面を開きます。
    /// </summary>
    public async Task Open(DbConnectionInfo connection)
    {
        var vm = DI.Resolve<ConnectionOperationViewModel>();

        using var view = new ConnectionOperationView(vm);

        await vm.Initialize(connection);
        view.SetConnectionTitle(connection);

        view.ShowDialog();
    }
}
