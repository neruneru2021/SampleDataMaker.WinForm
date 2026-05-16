using ChainingAssertion;
using Moq;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.WinForm.Services;
using SampleDataMaker.WinForm.ViewModels;

namespace SampleDataMaker.xUnit.ViewModelTests;

/// <summary>
/// MainViewModelの接続一覧読み込みと画面遷移の振る舞いを確認します。
///
/// ViewModelBaseはUIスレッドのSynchronizationContextを必要とするため、
/// TestInitializeでテスト用のSynchronizationContextを設定しています。
/// </summary>
[TestClass]
public sealed class MainViewModelTests
{
    [TestInitialize]
    public void Initialize()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [TestCleanup]
    public void Cleanup()
    {
        SynchronizationContext.SetSynchronizationContext(null);
    }

    [TestMethod]
    public async Task MainViewLoadは保存済み接続情報を一覧へ反映する()
    {
        // Arrange
        var connections = new[]
        {
            new DbConnectionInfo
            {
                Title = "Oracle PLM",
                DbType = DbTypeKind.Oracle,
                ConnectionString = "User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;",
                DefaultSchema = "PLMCONSOLE"
            }
        };
        var repositoryMock = new Mock<IDbConnectionInfoRepository>();
        repositoryMock
            .Setup(x => x.GetAll())
            .Returns(connections);
        var navigatorMock = new Mock<IConnectionOperationNavigator>();
        var vm = new MainViewModel(repositoryMock.Object, navigatorMock.Object);

        // Act
        await vm.MainViewLoad();

        // Assert
        vm.DgvConnectionsSource.Count.Is(1);
        vm.DgvConnectionsSource[0].Title.Is("Oracle PLM");
        vm.DgvConnectionsSource[0].DbType.Is(DbTypeKind.Oracle);
        vm.DgvConnectionsSource[0].DefaultSchema.Is("PLMCONSOLE");
    }

    [TestMethod]
    public async Task タイトル列クリック時は選択した接続で操作画面を開く()
    {
        // Arrange
        var connection = new DbConnectionInfo
        {
            Title = "SQL Server Docker",
            DbType = DbTypeKind.SqlServer,
            ConnectionString = "Server=localhost,1433;Database=SampleDb;"
        };
        var repositoryMock = new Mock<IDbConnectionInfoRepository>();
        var navigatorMock = new Mock<IConnectionOperationNavigator>();
        var vm = new MainViewModel(repositoryMock.Object, navigatorMock.Object);

        // Act
        await vm.DgvConnectionsCellContentClick("Title", connection);

        // Assert
        // MoqのVerifyは「この依存先が期待通り呼ばれたか」を確認する時に便利です。
        navigatorMock.Verify(x => x.Open(connection), Times.Once);
    }

    [TestMethod]
    public async Task タイトル列以外のクリックでは操作画面を開かない()
    {
        // Arrange
        var connection = new DbConnectionInfo
        {
            Title = "SQL Server Docker",
            DbType = DbTypeKind.SqlServer,
            ConnectionString = "Server=localhost,1433;Database=SampleDb;"
        };
        var repositoryMock = new Mock<IDbConnectionInfoRepository>();
        var navigatorMock = new Mock<IConnectionOperationNavigator>();
        var vm = new MainViewModel(repositoryMock.Object, navigatorMock.Object);

        // Act
        await vm.DgvConnectionsCellContentClick("ConnectionString", connection);

        // Assert
        navigatorMock.Verify(x => x.Open(It.IsAny<DbConnectionInfo>()), Times.Never);
    }
}
