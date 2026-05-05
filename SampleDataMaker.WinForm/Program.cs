using Microsoft.Extensions.DependencyInjection;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Infrastructure.Json;
using SampleDataMaker.WinForm.ViewModels;
using SampleDataMaker.WinForm.Views;

namespace SampleDataMaker.WinForm;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.Run(new MainView());
    }
}
