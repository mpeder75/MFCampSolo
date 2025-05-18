using CommunityToolkit.Aspire.Hosting.Dapr;
using Projects;
using System.Collections.Immutable;

var builder = DistributedApplication.CreateBuilder(args);

// Ingen statestore reference, da Dapr statestore ikke understøtter RavenDb
builder.AddProject<Order_API>("order-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "order-api",
        DaprHttpPort = 3502 ,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    });

// Shipping persister ingen data
builder.AddProject<Shipping_API>("shipping-api").
    WithDaprSidecar(new DaprSidecarOptions
{
    AppId = "shipping-api",
    DaprHttpPort = 3504,
    ResourcesPaths = ImmutableHashSet.Create("../resources")
});

builder.AddProject<Payment_API>("payment-api").
    WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "payment-api",
        DaprHttpPort = 3501,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    });

builder.Build().Run();
