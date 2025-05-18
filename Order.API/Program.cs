using Dapr.Client;
using Order.Application.Services;
using Order.Infrastructure;
using Order.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEventStoreClient(options => 
{
    options.ConnectivitySettings.Address = new Uri("esdb://localhost:2113");
    options.ConnectivitySettings.Insecure = true;
});

// Dapr
builder.Services.AddControllers().AddDapr();
builder.Services.AddDaprClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IEventPublisher>(sp => 
    new OrderEventPublisher(
        sp.GetRequiredService<DaprClient>(), 
        sp.GetRequiredService<ILogger<OrderEventPublisher>>()));

// Dapr pub/sub
builder.Services.AddScoped<OrderEventPublisher>();

// DependencyInjection configurations fil
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Understøtter Dapr CloudEvents
app.UseCloudEvents();
app.MapControllers();

// Dapr pub/sub 
app.MapSubscribeHandler();

app.Run();