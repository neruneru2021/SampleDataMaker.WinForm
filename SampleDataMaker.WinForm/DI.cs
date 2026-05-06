using Microsoft.Extensions.DependencyInjection;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.Domain.Services;
using SampleDataMaker.Infrastructure.Database;
using SampleDataMaker.Infrastructure.FileSystem;
using SampleDataMaker.Infrastructure.Json;
using SampleDataMaker.WinForm.Services;
using SampleDataMaker.WinForm.ViewModels;

namespace SampleDataMaker.WinForm
{
    /// <summary>
    /// 依存性注入
    /// </summary>
    internal static class DI
    {
        private static ServiceCollection _services = new();

        private static ServiceProvider _serviceProvider;

        static DI()
        {
            _services.AddSingleton<IDbConnectionInfoRepository, JsonDbConnectionInfoRepository>();
            _services.AddTransient<IConnectionOperationNavigator, ConnectionOperationNavigator>();
            _services.AddTransient<MainViewModel>();
            _services.AddTransient<ConnectionOperationViewModel>();
            _services.AddTransient<IDbTableInfoRepository, SqlServerDbTableInfoRepository>();
            _services.AddTransient<IDbTableSchemaRepository, SqlServerDbTableSchemaRepository>();
            _services.AddTransient<ITestDataGenerator, SimpleTestDataGenerator>();
            _services.AddTransient<IBoundaryTestDataGenerator, BoundaryTestDataGenerator>();
            _services.AddTransient<ITestDataOutputRepository, LocalTestDataOutputRepository>();
            _services.AddSingleton<ISampleDataRepository, JsonSampleDataRepository>();
            _services.AddTransient<IColumnSampleDataTemplateRepository, JsonColumnSampleDataTemplateRepository>();
            _serviceProvider = _services.BuildServiceProvider();
        }

        internal static T Resolve<T>()
            where T : notnull
        {
            // 無い時は例外
            return _serviceProvider.GetRequiredService<T>();

            // 無い時はnull
            // return _serviceProvider.GetService<T>();
        }
    }
}
