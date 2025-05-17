using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Order.Infrastructure.Repositories;

public class RavenDbContext
{
    private readonly IDocumentStore _store;

    public RavenDbContext(IDocumentStore store)
    {
        _store = store;
    }

    public IDocumentSession OpenSession()
    {
        return _store.OpenSession();
    }

    public IAsyncDocumentSession OpenAsyncSession()
    {
        return _store.OpenAsyncSession();
    }
}