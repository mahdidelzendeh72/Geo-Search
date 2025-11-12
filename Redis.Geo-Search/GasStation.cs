using Geo_Search.Interface;
using Geo_Search.Models;

namespace Geo_Search.Redis_GeoSearch
{
    public class GasStation : IGeoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Coordinate Coordinate { get; set; }
    }

}
