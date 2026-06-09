using Microsoft.Extensions.DependencyInjection;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Infrastructure.Json;
using SampleDataMaker.WinForm.ViewModels;
using SampleDataMaker.WinForm.Views;
using System.Reflection;

namespace SampleDataMaker.WinForm;

/// <summary>
/// WinFormsアプリケーションの起動処理を定義します。
/// </summary>
internal static class Program
{
    //// log4netにログに出すクラス名(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)をセット
    //private static readonly log4net.ILog _logger =
    //    log4net.LogManager.GetLogger(typeof(Program));

    /// <summary>
    /// アプリケーション設定を初期化し、メイン画面を表示します。
    /// </summary>
    [STAThread]
    static void Main()
    {
        ConfigureLog4Net();

        ApplicationConfiguration.Initialize();
        Application.ThreadException += (sender, e) =>
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //_logger.Error(e.Exception.Message, e.Exception);
        };

        //_logger.Info("Application started.");
        Application.Run(new MainView());
    }

    private static void ConfigureLog4Net()
    {
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs"));
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs", "Errors"));

        var repository = log4net.LogManager.GetRepository(typeof(Program).Assembly);
        var configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4.config"));

        log4net.Config.XmlConfigurator.Configure(repository, configFile);
    }
}
