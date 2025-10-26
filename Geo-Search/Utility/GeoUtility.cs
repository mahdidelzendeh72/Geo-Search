namespace Geo_Search.Utility
{
    public class GeoUtility
    {
        private const double EarthRadiusKm = 6371.0; // Earth's radius in km
        public static (double centerLat, double centerLon, double radiusKm) BoundingBoxToCircle(double minLat, double minLon, double maxLat, double maxLon)
        {
            // Center point of the bounding box
            double centerLat = (minLat + maxLat) / 2.0;
            double centerLon = (minLon + maxLon) / 2.0;

            // Distance from center to one corner of the box
            double radiusKm = HaversineDistance(centerLat, centerLon, maxLat, maxLon);

            return (centerLat, centerLon, radiusKm);
        }
        public static (double centerLat, double centerLon, double widthKm, double heightKm) CalculateBoundingBoxDimensions(double minLat, double minLon, double maxLat, double maxLon)
        {

            // Center point of the bounding box
            double centerLat = (minLat + maxLat) / 2.0;
            double centerLon = (minLon + maxLon) / 2.0;

            // Calculate the box dimensions
            var widthKm = HaversineDistance(minLat, minLon, minLat, maxLon);
            var heightKm = HaversineDistance(minLat, minLon, maxLat, minLon);
            return (centerLat, centerLon, widthKm, heightKm);


        }
        // Haversine distance formula
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Pow(Math.Sin(dLon / 2), 2);

            double c = 2 * Math.Asin(Math.Sqrt(a));

            return EarthRadiusKm * c;
        }
        private static double ToRadians(double angle) => Math.PI * angle / 180.0;
    }
}
