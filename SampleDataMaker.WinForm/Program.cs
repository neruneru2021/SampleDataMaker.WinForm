using Microsoft.Extensions.DependencyInjection;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Infrastructure.Json;
using SampleDataMaker.WinForm.ViewModels;
using SampleDataMaker.WinForm.Views;

namespace SampleDataMaker.WinForm;

/// <summary>
/// WinFormsアプリケーションの起動処理を定義します。
/// </summary>
internal static class Program
{
    /// <summary>
    /// アプリケーション設定を初期化し、メイン画面を表示します。
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.Run(new MainView());
    }
}
