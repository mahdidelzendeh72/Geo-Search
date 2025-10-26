namespace Geo_Search.Models
{
    public class GeoBoundingBox
    {
        public required Coordinate MinimumCoordinate { get; set; }
        public required Coordinate MaximumCoordinate { get; set; }
    }
}
