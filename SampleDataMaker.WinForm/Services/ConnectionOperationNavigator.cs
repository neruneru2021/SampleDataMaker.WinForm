using SampleDataMaker.Domain.Entities;
using SampleDataMaker.WinForm.ViewModels;
using SampleDataMaker.WinForm.Views;

namespace SampleDataMaker.WinForm.Services;

public class ConnectionOperationNavigator : IConnectionOperationNavigator
{
    public async Task Open(DbConnectionInfo connection)
    {
        var vm = DI.Resolve<ConnectionOperationViewModel>();

        using var view = new ConnectionOperationView(vm);

        await vm.Initialize(connection);

        view.ShowDialog();
    }
}
