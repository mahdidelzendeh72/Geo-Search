using Geo_Search.Interface;
using Geo_Search.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using StackExchange.Redis;
using System.Text.Json;
using NetCoordinate = NetTopologySuite.Geometries.Coordinate;

namespace Geo_Search.NetTopology_GeoSearch
{
    public class NetTopologyGeoSearch(IConnectionMultiplexer redis) : IGeoSearch<GasStation>
    {
        private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        private IDatabase GeoDb => redis.GetDatabase(1);
        const string GeoKey = "Gas-Stations-Geo-Data";

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        private static double CalculateDistanceKm(Models.Coordinate c1, Models.Coordinate c2)
        {
            const double R = 6371;
            var lat1 = ToRadians(c1.Latitude);
            var lat2 = ToRadians(c2.Latitude);
            var deltaLat = ToRadians(c2.Latitude - c1.Latitude);
            var deltaLon = ToRadians(c2.Longitude - c1.Longitude);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public async Task SetGeoData(List<GasStation> geoModels)
        {
            if (geoModels == null || geoModels.Count == 0)
                return;

            var batch = GeoDb.CreateBatch();
            var tasks = new List<Task>();

            await GeoDb.KeyDeleteAsync(GeoKey);

            foreach (var geo in geoModels)
            {
                var json = JsonSerializer.Serialize(geo);
                tasks.Add(batch.ListRightPushAsync(GeoKey, json));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task<List<GasStation>> SearchBaseBoundingBox(GeoBoundingBox boundingBox)
        {
            //var allJson = await GeoDb.ListRangeAsync(GeoKey, 0, -1);
            //var allStations = allJson
            //    .Select(x => JsonSerializer.Deserialize<GasStation>(x!))
            //    .Where(x => x != null)
            //    .ToList()!;

            //var envelope = new Envelope(
            //    boundingBox.MinimumCoordinate.Longitude,
            //    boundingBox.MaximumCoordinate.Longitude,
            //    boundingBox.MinimumCoordinate.Latitude,
            //    boundingBox.MaximumCoordinate.Latitude);

            //var bbox = _geometryFactory.ToGeometry(envelope);
            //var preparedBbox = PreparedGeometryFactory.Prepare(bbox);

            //var result = allStations
            //    .Where(gs =>
            //    {
            //        var pt = _geometryFactory.CreatePoint(new NetCoordinate(
            //            gs.Coordinate.Longitude, gs.Coordinate.Latitude));
            //        return preparedBbox.Contains(pt);
            //    })
            //    .ToList();

            //return result;


            var allJson = await GeoDb.ListRangeAsync(GeoKey, 0, -1);

            var allStations = allJson
                .Select(x =>
                {
                    if (x.IsNullOrEmpty) return null;
                    try
                    {
                        return JsonSerializer.Deserialize<GasStation>(x!);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(gs => gs != null && gs.Coordinate != null)
                .Cast<GasStation>()
                .ToList();

            var envelope = new Envelope(
                boundingBox.MinimumCoordinate.Longitude,
                boundingBox.MaximumCoordinate.Longitude,
                boundingBox.MinimumCoordinate.Latitude,
                boundingBox.MaximumCoordinate.Latitude);

            var bbox = _geometryFactory.ToGeometry(envelope);
            var preparedBbox = PreparedGeometryFactory.Prepare(bbox);

            var result = allStations
                .Where(gs =>
                {
                    var coord = gs.Coordinate!;
                    var pt = _geometryFactory.CreatePoint(new NetCoordinate(coord.Longitude, coord.Latitude));
                    return preparedBbox.Contains(pt);
                })
                .ToList();

            return result;
        }

        public async Task<GasStation?> FindNearestGeoModel(Models.Coordinate coordinate)
        {
            var allJson = await GeoDb.ListRangeAsync(GeoKey, 0, -1);
            var allStations = allJson
                .Select(x => JsonSerializer.Deserialize<GasStation>(x!))
                .Where(x => x != null)
                .ToList()!;

            if (allStations.Count == 0)
                return null;

            const double MaxKm = 10.0;
            GasStation? nearest = null;
            return await Task.Run(() =>
            {

                double minDistanceKm = double.MaxValue;

                foreach (var station in allStations)
                {
                    if (station?.Coordinate == null)
                        continue;
                    var distKm = CalculateDistanceKm(coordinate, station.Coordinate);
                    if (distKm < minDistanceKm)
                    {
                        minDistanceKm = distKm;
                        nearest = station;
                    }
                }

                return minDistanceKm <= MaxKm ? nearest : null;
            });

        }
    }
}
