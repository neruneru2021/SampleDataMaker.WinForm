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
    /// アプリで使用するサービスとViewModelの依存関係を一元管理します。
    /// </summary>
    internal static class DI
    {
        private static ServiceCollection _services = new();

        private static ServiceProvider _serviceProvider;

        /// <summary>
        /// アプリ全体で使用するサービスとViewModelを登録します。
        /// </summary>
        static DI()
        {
            _services.AddSingleton<IDbConnectionInfoRepository, JsonDbConnectionInfoRepository>();
            _services.AddTransient<IConnectionOperationNavigator, ConnectionOperationNavigator>();
            _services.AddTransient<MainViewModel>();
            _services.AddTransient<ConnectionOperationViewModel>();
            _services.AddTransient<IDbTableInfoRepository, CompositeDbTableInfoRepository>();
            _services.AddTransient<IDbTableSchemaRepository, CompositeDbTableSchemaRepository>();
            _services.AddTransient<ITestDataGenerator, SimpleTestDataGenerator>();
            _services.AddTransient<IBoundaryTestDataGenerator, BoundaryTestDataGenerator>();
            _services.AddTransient<ITestDataOutputRepository, LocalTestDataOutputRepository>();
            _services.AddTransient<ITestDataDirectInsertRepository, DbTestDataDirectInsertRepository>();
            _services.AddTransient<IExistingKeyValueRepository, DbExistingKeyValueRepository>();
            _services.AddSingleton<ISampleDataRepository, JsonSampleDataRepository>();
            _services.AddTransient<IColumnSampleDataTemplateRepository, JsonColumnSampleDataTemplateRepository>();
            _services.AddSingleton<IForeignKeyRelationRepository, JsonForeignKeyRelationRepository>();
            _services.AddTransient<IForeignKeyTestDataApplier, ForeignKeyTestDataApplier>();
            _services.AddTransient<IForeignKeyTypeMismatchConfirmationService, ForeignKeyTypeMismatchConfirmationService>();
            _services.AddTransient<ForeignKeySelectViewModel>();
            _serviceProvider = _services.BuildServiceProvider();
        }

        /// <summary>
        /// 登録済みサービスをDIコンテナから取得します。
        /// </summary>
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
