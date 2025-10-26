using Geo_Search.Interface;
using Geo_Search.Redis_GeoSearch;
using Microsoft.OpenApi.Models;
using Sample;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


// Register Redis Connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// Register Geo Search Service
builder.Services.AddScoped<IGeoSearch<GasStation>, RedisGeoSearch>();
// Background Seeder
builder.Services.AddHostedService<GasStationGeoSeeder>();

// Controllers
builder.Services.AddControllers();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gas Station Geo API",
        Version = "v1",
        Description = "API for searching gas stations using Redis GEO features."
    });
});

var app = builder.Build();

// ✅ Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gas Station Geo API v1");
    c.RoutePrefix = string.Empty; // Swagger at root URL
});
app.MapControllers();
app.Run();
