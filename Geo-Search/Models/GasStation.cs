using Geo_Search.Interface;

namespace Geo_Search.Models
{
    public class GasStation : IGeoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Coordinate Coordinate { get; set; } = new Coordinate();
    }
}
