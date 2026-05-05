using SampleDataMaker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleDataMaker.Domain.Repositories
{
    public interface ITableInfoRepository
    {
        IReadOnlyList<TableInfoEntity> GetITableInfoList();
    }
}
