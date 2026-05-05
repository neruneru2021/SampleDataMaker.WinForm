using SampleDataMaker.Domain.Enums;

namespace SampleDataMaker.Domain.Entities
{
    public class DbConnectionInfo
    {
        public string Title { get; set; } = string.Empty;

        public DbTypeKind DbType { get; set; }

        public string ConnectionString { get; set; } = string.Empty;
    }
}
