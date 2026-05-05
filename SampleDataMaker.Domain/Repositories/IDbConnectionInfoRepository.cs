using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;
public interface IDbConnectionInfoRepository
{
    IReadOnlyList<DbConnectionInfo> GetAll();

    void SaveAll(IReadOnlyList<DbConnectionInfo> connections);
}
