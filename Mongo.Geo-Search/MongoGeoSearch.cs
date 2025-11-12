using Geo_Search.Interface;
using Geo_Search.Models;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Mongo.Geo_Search
{
    public class MongoGeoSearch : IGeoSearch<PetShopDto>
    {
        public MongoGeoSearch(IMongoDatabase database)
        {
            _collection = database.GetCollection<PetShopDocument>("petshop");
        }
        private readonly IMongoCollection<PetShopDocument> _collection;
        public async Task<PetShopDto?> FindNearestGeoModel(Coordinate coordinate)
        {
            var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(coordinate.Longitude, coordinate.Latitude));
            var filter = Builders<PetShopDocument>.Filter.NearSphere(x => x.Location, point);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            if (result == null)
            {
                return null;
            }
            return new PetShopDto
            {
                Name = result.Name,
                Coordinate = new Coordinate
                {
                    Latitude = result.Location.Coordinates.Latitude,
                    Longitude = result.Location.Coordinates.Longitude
                }
            };

        }

        public async Task<List<PetShopDto>> SearchBaseBoundingBox(GeoBoundingBox boundingBox)
        {
            var lowerLeft = new GeoJson2DGeographicCoordinates(
                        boundingBox.MinimumCoordinate.Longitude, boundingBox.MinimumCoordinate.Latitude);

            var upperRight = new GeoJson2DGeographicCoordinates(
                boundingBox.MaximumCoordinate.Longitude, boundingBox.MaximumCoordinate.Latitude);

            var polygon = new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
                    new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(
                          new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(new[]
                          {
                              new GeoJson2DGeographicCoordinates(lowerLeft.Longitude, lowerLeft.Latitude),
                              new GeoJson2DGeographicCoordinates(lowerLeft.Longitude, upperRight.Latitude),
                              new GeoJson2DGeographicCoordinates(upperRight.Longitude, upperRight.Latitude),
                              new GeoJson2DGeographicCoordinates(upperRight.Longitude, lowerLeft.Latitude),
                              new GeoJson2DGeographicCoordinates(lowerLeft.Longitude, lowerLeft.Latitude) // close ring
                          })
                    )
            );


            var filter = Builders<PetShopDocument>.Filter.GeoWithin(x => x.Location, polygon);
            var result = await _collection.Find(filter).ToListAsync();
            return result.Select(doc => new PetShopDto
            {
                Name = doc.Name,
                Coordinate = new Coordinate
                {
                    Latitude = doc.Location.Coordinates.Latitude,
                    Longitude = doc.Location.Coordinates.Longitude
                }
            }).ToList() ?? [];
        }

        public async Task SetGeoData(List<PetShopDto> geoModels)
        {
            var docs = geoModels.Select(model => new PetShopDocument
            {
                Name = model.Name,
                Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                    new GeoJson2DGeographicCoordinates(model.Coordinate.Longitude, model.Coordinate.Latitude))
            }).ToList();
            await _collection.DeleteManyAsync(Builders<PetShopDocument>.Filter.Empty);
            await _collection.InsertManyAsync(docs);
        }
    }
}
