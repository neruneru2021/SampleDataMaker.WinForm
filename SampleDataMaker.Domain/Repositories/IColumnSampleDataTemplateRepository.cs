using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Repositories;

public interface IColumnSampleDataTemplateRepository
{
    Task SaveAsync(
        ColumnSampleDataTemplate template,
        CancellationToken cancellationToken = default);
}
