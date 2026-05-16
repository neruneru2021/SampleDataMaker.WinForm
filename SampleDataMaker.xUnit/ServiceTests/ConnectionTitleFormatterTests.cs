using ChainingAssertion;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;
using SampleDataMaker.WinForm.Services;

namespace SampleDataMaker.xUnit.ServiceTests;

/// <summary>
/// 画面タイトル用の接続先表示文字列を確認します。
///
/// タイトルは小さな機能ですが、利用者が「今どこに接続しているか」を判断する重要な情報です。
/// DB種別ごとの接続文字列から、必要な情報だけを抜き出せていることをテストします。
/// </summary>
[TestClass]
public sealed class ConnectionTitleFormatterTests
{
    [TestMethod]
    public void Oracle操作画面タイトルはホストサービススキーマを表示する()
    {
        // Arrange
        var connection = new DbConnectionInfo
        {
            DbType = DbTypeKind.Oracle,
            ConnectionString = "User Id=system;Password=oracle;Data Source=localhost:1521/XEPDB1;",
            DefaultSchema = "PLMCONSOLE"
        };

        // Act
        var title = ConnectionTitleFormatter.CreateOperationTitle(connection);

        // Assert
        title.Is("テーブル操作 - Oracle | Host=localhost:1521 / Service=XEPDB1 / Schema=PLMCONSOLE");
    }

    [TestMethod]
    public void 外部キー設定画面タイトルは接続先と設定元カラムを表示する()
    {
        // Arrange
        var connection = new DbConnectionInfo
        {
            DbType = DbTypeKind.SqlServer,
            ConnectionString = "Server=localhost,1433;Database=SampleDb;"
        };
        var sourceColumn = new DbColumnInfo
        {
            SchemaName = "dbo",
            TableName = "Users",
            ColumnName = "ClinicId"
        };

        // Act
        var title = ConnectionTitleFormatter.CreateForeignKeyTitle(connection, sourceColumn);

        // Assert
        title.Is("外部キー設定 - SqlServer | Host=localhost,1433 / Database=SampleDb | Source=dbo.Users.ClinicId");
    }
}
