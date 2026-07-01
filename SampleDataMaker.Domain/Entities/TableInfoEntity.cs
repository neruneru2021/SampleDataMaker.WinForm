using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleDataMaker.Domain.Entities
{
    /// <summary>
    /// テーブル一覧に表示するSQL Serverテーブルの識別情報を表します。
    /// </summary>
    public sealed class TableInfoEntity //: ITableInfoEntity
    {
        public int ObjectId { get; set; }

        public string SchemaName { get; set; } = "";

        public string TableName { get; set; } = "";

        public string SafeFullName { get; set; } = "";

        public string DisplayName { get; set; } = "";
    }
}
