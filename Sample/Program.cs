using Microsoft.OpenApi.Models;
using Mongo.Geo_Search;
using Sample.Redis;

var builder = WebApplication.CreateBuilder(args);

//use redis >>>> uncomment bellow code
//builder.Services.RegisterRedisGeoServices(builder.Configuration);

//use mongo
builder.Services.RegisterMongoGeoServices(builder.Configuration);


// Controllers
builder.Services.AddControllers();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Geo Search API",
        Version = "v1",
        Description = "API for geo searching ."
    });
});

var app = builder.Build();

// ✅ Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Geo Search API");
    c.RoutePrefix = string.Empty; // Swagger at root URL
});
app.MapControllers();
app.Run();
