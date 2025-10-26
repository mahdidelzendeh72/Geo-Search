using Geo_Search.Models;

namespace Geo_Search.Interface
{
    public interface IGeoSearch<TGeoModel> where TGeoModel : IGeoModel
    {
        Task<List<TGeoModel>> SearchBaseBoundingBox(GeoBoundingBox boundingBox);
        Task<TGeoModel?> FindNearestGeoModel(Coordinate coordinate);
        Task SetGeoData(List<TGeoModel> geoModels);
    }
    public interface IGeoModel
    {
        public Coordinate Coordinate { get; set; }
    }
}
