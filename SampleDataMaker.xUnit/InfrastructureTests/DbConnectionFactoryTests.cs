using ChainingAssertion;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.Infrastructure.Database;

namespace SampleDataMaker.xUnit.InfrastructureTests;

/// <summary>
/// DbTypeKindから実際のDbConnection実装へ変換するファクトリを確認します。
///
/// ここでは接続をOpenしません。
/// 生成される型だけを見ることで、DBサーバーが起動していなくても安全にテストできます。
/// </summary>
[TestClass]
public sealed class DbConnectionFactoryTests
{
    [TestMethod]
    public void SqlServer指定時はSqlConnectionを作成する()
    {
        // Arrange
        var connectionInfo = new DbConnectionInfo
        {
            DbType = DbTypeKind.SqlServer,
            ConnectionString = "Server=localhost,1433;Database=SampleDb;"
        };

        // Act
        using var connection = DbConnectionFactory.Create(connectionInfo);

        // Assert
        connection.GetType().FullName.Is("Microsoft.Data.SqlClient.SqlConnection");
    }

    [TestMethod]
    public void Oracle指定時はOracleConnectionを作成する()
    {
        // Arrange
        var connectionInfo = new DbConnectionInfo
        {
            DbType = DbTypeKind.Oracle,
            ConnectionString = "User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;"
        };

        // Act
        using var connection = DbConnectionFactory.Create(connectionInfo);

        // Assert
        connection.GetType().FullName.Is("Oracle.ManagedDataAccess.Client.OracleConnection");
    }
}
