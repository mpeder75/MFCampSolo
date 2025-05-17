var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Dapr
builder.Services.AddControllers().AddDapr();
builder.Services.AddOpenApi();

var app = builder.Build();

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