using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.ReadModels;
using Order.Application.Services;
using Order.Domain.Events;
using Order.Domain.Repositories;
using Order.Infrastructure.Events;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Projections;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Services;
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
                FindIdentityProperty = memberInfo => memberInfo.Name == "DocumentId"
            }
        };

        store.Initialize();

        // Create database if it doesn't exist
        try
        {
            store.Maintenance.Server.Send(new CreateDatabaseOperation(
                new DatabaseRecord(store.Database)));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            // Ignore if database already exists
        }

        // Register DocumentStore as singleton
        services.AddSingleton<IDocumentStore>(store);

        // Register repositories and services
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();
        services.AddScoped<IEventSerializer, JsonEventSerializer>();
        services.AddScoped<EventStoreDb>();
        services.AddScoped<RavenDbContext>();
        services.AddScoped<IOrderQueries, RavenOrderQueries>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IEventPublisher, OrderEventPublisher>();

        // New outbox pattern components
        services.AddScoped<IOutboxService, RavenDbOutboxService>();
        services.AddScoped<DomainEventOutboxHandler>();
        services.AddHostedService<OutboxProcessorService>();

        // Event projections for read models
        services.AddScoped<IProjection, OrderSummaryProjector>();
        services.AddScoped<IProjection, OrderDetailsProjector>();
        services.AddScoped<OrderEventProjector>();

        return services;
    }
}