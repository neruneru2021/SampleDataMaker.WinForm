using SampleDataMaker.Domain.Enums;

namespace SampleDataMaker.Domain.Entities
{
    /// <summary>
    /// 接続先DBの種類、接続文字列、表示名などの接続情報を表します。
    /// </summary>
    public class DbConnectionInfo
    {
        public string Title { get; set; } = string.Empty;

        public DbTypeKind DbType { get; set; }

        public string ConnectionString { get; set; } = string.Empty;

        public string DefaultSchema { get; set; } = string.Empty;
    }
}
