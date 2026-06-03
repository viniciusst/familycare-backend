namespace FamilyCare.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides:
///  - A fresh <see cref="HttpClient"/> per test.
///  - Automatic database reset before each test (via <see cref="IAsyncLifetime"/>).
///  - Convenience accessor to the factory for helpers that need raw DB access.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected FamilyCareApiFactory Factory { get; }
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase(FamilyCareApiFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        Client = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}
