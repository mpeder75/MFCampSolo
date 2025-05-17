using Customer.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();




builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers().AddDapr();
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDB"));
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer("keycloak", realm: "MFCamp", options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = "account";
    });

builder.Services.AddDaprClient();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCloudEvents();
app.MapControllers();

app.MapSubscribeHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.Run();
