using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.ReadModels;
using Order.Domain.Events;
using Order.Domain.Repositories;
using Order.Infrastructure;
using Order.Infrastructure.Events;
using Order.Infrastructure.Indexes;
using Order.Infrastructure.Repositories;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.EventStoreDb;
using Testcontainers.RavenDb;
using Xunit;

namespace Order.IntegrationTests.Infrastructure;

public class TestFixture : IAsyncLifetime
{
    private readonly EventStoreDbContainer _eventStoreContainer;
    private readonly RavenDbContainer _ravenDbContainer;
    private ServiceProvider _serviceProvider;

    public TestFixture()
    {
        // Opsæt EventStore container
        _eventStoreContainer = new EventStoreDbBuilder()
            .WithImage("eventstore/eventstore:latest")
            .WithEnvironment("EVENTSTORE_INSECURE", "true")
            .WithEnvironment("EVENTSTORE_CLUSTER_SIZE", "1")
            .WithEnvironment("EVENTSTORE_RUN_PROJECTIONS", "All")
            .WithEnvironment("EVENTSTORE_START_STANDARD_PROJECTIONS", "true")
            .WithPortBinding(2113, true)
            .WithPortBinding(1113, true)
            .Build();

        // Opsæt RavenDB container
        _ravenDbContainer = new RavenDbBuilder()
            .WithImage("ravendb/ravendb:latest")
            .WithEnvironment("RAVEN_Security_UnsecuredAccessAllowed", "PublicNetwork")
            .WithEnvironment("RAVEN_Setup_Mode", "None")
            .WithEnvironment("RAVEN_License_Eula_Accepted", "true")
            .WithPortBinding(8080, true)
            .Build();
    }

    public async Task InitializeAsync()
{
    // Start containerne
    await _eventStoreContainer.StartAsync();
    await _ravenDbContainer.StartAsync();

    await Task.Delay(10000); // Giv containerne tid til at starte - 10 sekunder

    // Opsæt konfiguration med containere til brug for tests
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ConnectionStrings:EventStore", _eventStoreContainer.GetConnectionString() },
            { "ConnectionStrings:RavenDB", _ravenDbContainer.GetConnectionString() },
            { "RavenDB:DatabaseName", "OrdersTest" }
        }).Build();

    // Opsæt ServiceCollection med vores infrastruktur
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(configuration);
    
    // Opret EventStore klienten
    var eventStoreConnectionString = _eventStoreContainer.GetConnectionString();
    var settings = EventStoreClientSettings.Create(eventStoreConnectionString);
    services.AddSingleton(new EventStoreClient(settings));
    
    // Opret RavenDB DocumentStore med custom konventioner
    var store = new DocumentStore
    {
        Urls = new[] { _ravenDbContainer.GetConnectionString() },
        Database = "OrdersTest",
        Conventions = new DocumentConventions
        {
            // Denne linje fortæller RavenDB at bruge DocumentId egenskaben til ID
            FindIdentityProperty = memberInfo => memberInfo.Name == "DocumentId"
        }
    };
    
    store.Initialize();

    try
    {
        store.Maintenance.Server.Send(new Raven.Client.ServerWide.Operations.CreateDatabaseOperation(
            new Raven.Client.ServerWide.DatabaseRecord("OrdersTest")));
        Console.WriteLine("Database 'OrdersTest' oprettet");
    }
    catch (Exception ex) when (ex.Message.Contains("already exists"))
    {
        Console.WriteLine("Database 'OrdersTest' findes allerede");
    }


    services.AddSingleton<IDocumentStore>(store);
    
    // Registrer repositories og services
    services.AddScoped<IEventStoreRepository, EventStoreRepository>();
    services.AddScoped<IEventSerializer, TestJsonEventSerializer>();
    services.AddScoped<EventStoreDb>();
    services.AddScoped<RavenDbContext>();
    services.AddScoped<IOrderQueries, RavenOrderQueries>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<OrderEventProjector>();
    
    // Byg service provider
    _serviceProvider = services.BuildServiceProvider();
    
    // Opret indekser
    try
    {
        IndexCreation.CreateIndexes(typeof(Orders_ByCustomerId).Assembly, store);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fejl ved oprettelse af indekser: {ex.Message}");
    }
}

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    
        // Nedluk containere
        try 
        {
            if (_ravenDbContainer != null)
            {
                await _ravenDbContainer.DisposeAsync();
            }
        
            if (_eventStoreContainer != null)
            {
                await _eventStoreContainer.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved nedlukning af containere: {ex.Message}");
        }
    }

    // Hjælpemetode til at få services fra DI containeren
    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}