using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.Services;

public interface IConnectionOperationNavigator
{
    Task Open(DbConnectionInfo connection);
}

