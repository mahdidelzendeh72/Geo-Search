using Geo_Search.Interface;
using Geo_Search.Models;
using Geo_Search.Utility;
using StackExchange.Redis;
using System.Text.Json;

namespace Geo_Search.Redis_GeoSearch
{
    public class RedisGeoSearch(IConnectionMultiplexer redis) : IGeoSearch<GasStation>
    {
        private IDatabase _geoDb => redis.GetDatabase(1);
        const string GeoKey = "Gas-Stations-Geo-Data";
        public async Task<List<GasStation>> SearchBaseBoundingBox(GeoBoundingBox boundingBox)
        {
            var (centerLat, centerLon, widthKm, heightKm) = GeoUtility.CalculateBoundingBoxDimensions(boundingBox.MinimumCoordinate.Latitude,
                                                                                                      boundingBox.MinimumCoordinate.Longitude,
                                                                                                      boundingBox.MaximumCoordinate.Latitude,
                                                                                                      boundingBox.MaximumCoordinate.Longitude);

            var results = await _geoDb.GeoSearchAsync(GeoKey, centerLon, centerLat, new GeoSearchBox(heightKm, widthKm, GeoUnit.Kilometers));
            var allGasStation = results?.Select(r => JsonSerializer.Deserialize<GasStation>(r.Member!.ToString())).Where(h => h != null).ToList() ?? [];

            return allGasStation;

        }

        public async Task SetGeoData(List<GasStation> geoModels)
        {
            if (geoModels == null || geoModels.Count == 0)
                return;
            var batch = _geoDb.CreateBatch();
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

            GeoRadiusResult[] results = await _geoDb.GeoSearchAsync(
                key: GeoKey,
                longitude: coordinate.Longitude,
                latitude: coordinate.Latitude,
                shape: shape,
                count: 1,
                order: Order.Ascending,
                options: GeoRadiusOptions.WithCoordinates);

            var nearest = results?.FirstOrDefault();
            return nearest == null
                ? null
                : JsonSerializer.Deserialize<GasStation>(nearest!.ToString());
        }
    }
    public class GasStation : IGeoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Coordinate Coordinate { get; set; }
    }

}
