namespace Sample.Redis
{
    using Geo_Search.Interface;
    using Geo_Search.Models;
    using Geo_Search.Redis_GeoSearch;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class GasStationGeoSeeder : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<GasStationGeoSeeder> _logger;

        public GasStationGeoSeeder(IServiceScopeFactory serviceScopeFactory, ILogger<GasStationGeoSeeder> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⛽ GasStation seeder service starting...");

            // Create a DI scope so we can resolve scoped services like IGeoSearch<GasStation>
            using var scope = _serviceScopeFactory.CreateScope();
            var geoSearch = scope.ServiceProvider.GetRequiredService<IGeoSearch<GasStation>>();

            try
            {
                var stations = GenerateSampleStations();
                await geoSearch.SetGeoData(stations);

                _logger.LogInformation("✅ Seeded {Count} gas stations into Redis GEO data.", stations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error seeding gas stations into Redis.");
            }

            _logger.LogInformation("🏁 GasStation seeder service completed.");
        }

        private List<GasStation> GenerateSampleStations()
        {
            // Example: random stations near New York City
            var baseLat = 40.7128;
            var baseLon = -74.0060;
            var random = new Random();

            var stations = new List<GasStation>();

            for (int i = 1; i <= 10; i++)
            {
                var offsetLat = (random.NextDouble() - 0.5) * 0.1; // ~10 km variation
                var offsetLon = (random.NextDouble() - 0.5) * 0.1;

                stations.Add(new GasStation
                {
                    Id = i,
                    Name = $"Gas Station #{i}",
                    Coordinate = new Coordinate
                    {
                        Latitude = baseLat + offsetLat,
                        Longitude = baseLon + offsetLon
                    }
                });
            }

            return stations;
        }
    }

}
