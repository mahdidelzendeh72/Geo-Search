using Geo_Search.Interface;
using Geo_Search.Models;
using Geo_Search.Utility;
using StackExchange.Redis;
using System.Text.Json;

namespace Geo_Search.Redis_GeoSearch
{
    public class RedisGeoSearch(IConnectionMultiplexer redis) : IGeoSearch<GasStation>
    {
        private IDatabase GeoDb => redis.GetDatabase(1);
        const string GeoKey = "Gas-Stations-Geo-Data";
        public async Task<List<GasStation>> SearchBaseBoundingBox(GeoBoundingBox boundingBox)
        {
            var (centerLat, centerLon, widthKm, heightKm) =
                GeoUtility.CalculateBoundingBoxDimensions(
                    boundingBox.MinimumCoordinate.Latitude,
                    boundingBox.MinimumCoordinate.Longitude,
                    boundingBox.MaximumCoordinate.Latitude,
                    boundingBox.MaximumCoordinate.Longitude);

            var results = await GeoDb.GeoSearchAsync(
                GeoKey,
                centerLon,
                centerLat,
                new GeoSearchBox(heightKm, widthKm, GeoUnit.Kilometers));

            var allGasStations = results?
                .Select(r => JsonSerializer.Deserialize<GasStation>(r.Member!.ToString()))
                .OfType<GasStation>()
                .ToList()
                ?? [];

            return allGasStations;

        }

        public async Task SetGeoData(List<GasStation> geoModels)
        {
            if (geoModels == null || geoModels.Count == 0)
                return;
            var batch = GeoDb.CreateBatch();
            var tasks = new List<Task>();

            foreach (var geo in geoModels)
            {
                tasks.Add(batch.GeoAddAsync(
                   GeoKey,
                   new GeoEntry(geo.Coordinate.Longitude, geo.Coordinate.Latitude, JsonSerializer.Serialize(geo))
               ));
            }
            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task<GasStation?> FindNearestGeoModel(Coordinate coordinate)
        {
            var shape = new GeoSearchCircle(10, GeoUnit.Kilometers); // radius in km

            GeoRadiusResult[] results = await GeoDb.GeoSearchAsync(
                key: GeoKey,
                longitude: coordinate.Longitude,
                latitude: coordinate.Latitude,
                shape: shape,
                count: 1,
                order: Order.Ascending,
                options: GeoRadiusOptions.WithCoordinates);

            var nearest = results?.FirstOrDefault();
            if (nearest?.Member is null)
                return null;

            var json = nearest.ToString();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var gasStation = JsonSerializer.Deserialize<GasStation>(json);
            return gasStation;
        }
    }
}
