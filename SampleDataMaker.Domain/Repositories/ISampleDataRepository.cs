namespace SampleDataMaker.Domain.Repositories;

public interface ISampleDataRepository
{
    IReadOnlyList<string> GetKinds();

    IReadOnlyList<string> GetValues(string kind);
}
