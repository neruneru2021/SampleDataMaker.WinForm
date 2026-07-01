using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Infrastructure.SqlServer;

/// <summary>
/// SQL Serverのテーブル情報を返す旧形式のRepository実装です。
/// </summary>
internal class TableInfoSqlServer: ITableInfoRepository
{
    public IReadOnlyList<TableInfoEntity> GetITableInfoList()
    {
        var list = new List<TableInfoEntity>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(new TableInfoEntity
            {
                ObjectId = i,
                SchemaName = $"Schema{i}",
                TableName = $"Table{i}",
                SafeFullName = $"[Schema{i}].[Table{i}]",
                DisplayName = $"Schema{i}.Table{i}"
            });
        }
        return list;
    }
}
