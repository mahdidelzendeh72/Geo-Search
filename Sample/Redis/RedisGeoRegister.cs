using Geo_Search.Interface;
using Geo_Search.Redis_GeoSearch;
using StackExchange.Redis;

namespace Sample.Redis
{
    public static class RedisGeoExtensions
    {
        public static IServiceCollection RegisterRedisGeoServices(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(redis);
            services.AddScoped<IGeoSearch<GasStation>, RedisGeoSearch>();
            services.AddHostedService<GasStationGeoSeeder>();
            return services;
        }
    }
}
