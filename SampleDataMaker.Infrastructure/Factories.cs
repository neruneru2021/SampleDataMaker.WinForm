using SampleDataMaker.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleDataMaker.Infrastructure
{
    /// <summary>
    /// Infrastructure層のRepository実装を生成します。
    /// </summary>
    public static class Factories
    {
        public static ITableInfoRepository CreateTableInfoRepository()
        {
            return new SqlServer.TableInfoSqlServer();
        }
    }
}
