using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.ReadModels;
using Order.Domain.Events;
using Order.Domain.Repositories;
using Order.Infrastructure.Events;
using Order.Infrastructure.Projections;
using Order.Infrastructure.Repositories;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EventStore client
        var eventStoreConnectionString = configuration.GetConnectionString("EventStore");

        if (string.IsNullOrEmpty(eventStoreConnectionString))
        {
            eventStoreConnectionString = "esdb://localhost:2113?tls=false";
        }

        var settings = EventStoreClientSettings.Create(eventStoreConnectionString);
        services.AddSingleton(new EventStoreClient(settings));

        // RavenDB
        var store = new DocumentStore
        {
            Urls = new[] { configuration.GetConnectionString("RavenDB") ?? "http://localhost:8080" },
            Database = configuration.GetSection("RavenDB")["DatabaseName"] ?? "Orders",
            Conventions = new DocumentConventions
            {
                // Denne linje fortæller RavenDB at bruge DocumentId egenskaben til ID
                FindIdentityProperty = memberInfo => memberInfo.Name == "DocumentId"
            }
        };

        store.Initialize();

        // Registrer indekser INDEN database tjek
        //IndexCreation.CreateIndexes(typeof(Orders_ByCustomerId).Assembly, store);

        // Opret database hvis den ikke findes
        try
        {
            store.Maintenance.Server.Send(new CreateDatabaseOperation(
                new DatabaseRecord(store.Database)));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            // Ignorér hvis databasen allerede findes
        }

        // Registrer DocumentStore som singleton
        services.AddSingleton<IDocumentStore>(store);

        // Register repositories and services
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();
        services.AddScoped<IEventSerializer, JsonEventSerializer>();
        services.AddScoped<EventStoreDb>();
        services.AddScoped<RavenDbContext>();
        services.AddScoped<IOrderQueries, RavenOrderQueries>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProjection, OrderSummaryProjector>();
        services.AddScoped<IProjection, OrderDetailsProjector>();

        // Event projection
        services.AddScoped<OrderEventProjector>();

        services.AddTransient<Action<ProjectionManager>>(sp => manager => {
            manager.RegisterProjection(sp.GetRequiredService<OrderSummaryProjector>());
            manager.RegisterProjection(sp.GetRequiredService<OrderDetailsProjector>());
        });

        return services;
    }
}