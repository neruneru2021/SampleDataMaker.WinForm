using System.Data.Common;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Enums;

namespace SampleDataMaker.Infrastructure.Database;

/// <summary>
/// DB種別に対応したデータベース接続を生成します。
/// </summary>
public static class DbConnectionFactory
{
    public static DbConnection Create(DbConnectionInfo info)
    {
        return info.DbType switch
        {
            DbTypeKind.SqlServer => new SqlConnection(info.ConnectionString),
            DbTypeKind.Oracle => new OracleConnection(info.ConnectionString),

            _ => throw new NotSupportedException($"未対応のDB種別です: {info.DbType}")
        };
    }
}
