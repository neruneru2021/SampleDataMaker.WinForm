using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface IColumnSampleDataTemplateRepository
{
    IReadOnlyList<ColumnSampleDataTemplate> GetAll();

    Task SaveAsync(
        ColumnSampleDataTemplate template,
        CancellationToken cancellationToken = default);
}
