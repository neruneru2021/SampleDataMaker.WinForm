using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleDataMaker.Domain.Entities
{
    public sealed class TableInfoEntity //: ITableInfoEntity
    {
        public int ObjectId { get; set; }

        public string SchemaName { get; set; } = "";

        public string TableName { get; set; } = "";

        public string SafeFullName { get; set; } = "";

        public string DisplayName { get; set; } = "";
    }
}