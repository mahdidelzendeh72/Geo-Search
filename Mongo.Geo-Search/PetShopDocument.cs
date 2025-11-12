using Geo_Search.Interface;
using Geo_Search.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Mongo.Geo_Search
{
    public class PetShopDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        [BsonElement("location")]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
    }

    public class PetShopDto : IGeoModel
    {

        public string Name { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}
