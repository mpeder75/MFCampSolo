using CommunityToolkit.Aspire.Hosting.Dapr;

using Projects;
using System.Collections.Immutable;

var builder = DistributedApplication.CreateBuilder(args);

// Dapr statestores
var customerStatestore = builder.AddDaprStateStore("customerstatestore");
var warehouseStatestore = builder.AddDaprStateStore("warehousestatestore");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume()
    .WithExternalHttpEndpoints();

builder.AddProject<Customer_API>("customer-api").
    WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "customer-api",
        DaprHttpPort = 3505,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    })
    .WithReference(customerStatestore)
    .WithReference(keycloak)
    .WaitFor(keycloak);

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
    
builder.AddProject<Warehouse_API>("warehouse-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "warehouse-api",
        DaprHttpPort = 3503,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    })
    .WithReference(warehouseStatestore);

// Payment persister ingen data
builder.AddProject<Payment_API>("payment-api").
    WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "payment-api",
        DaprHttpPort = 3501,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    });

builder.AddProject<Workflow_API>("workflow-api")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "workflow-api",
        DaprHttpPort = 3506,
        ResourcesPaths = ImmutableHashSet.Create("../resources")
    });


builder.Build().Run();
