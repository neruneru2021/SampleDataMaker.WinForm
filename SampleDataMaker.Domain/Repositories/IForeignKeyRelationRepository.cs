using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface IForeignKeyRelationRepository
{
    IReadOnlyList<ForeignKeyRelationSetting> GetAll();

    Task SaveAllAsync(
        IReadOnlyList<ForeignKeyRelationSetting> settings,
        CancellationToken cancellationToken = default);
}
