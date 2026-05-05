using SampleDataMaker.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleDataMaker.Infrastructure
{
    public static class Factories
    {
        public static ITableInfoRepository CreateTableInfoRepository()
        {
            return new SqlServer.TableInfoSqlServer();
        }
    }
}
