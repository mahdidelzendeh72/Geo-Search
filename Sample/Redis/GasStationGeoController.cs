namespace Sample.Redis
{
    using Geo_Search.Interface;
    using Geo_Search.Models;
    using Geo_Search.Redis_GeoSearch;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class GasStationGeoController : ControllerBase
    {
        private readonly IGeoSearch<GasStation> _geoSearch;

        public GasStationGeoController(IGeoSearch<GasStation> geoSearch)
        {
            _geoSearch = geoSearch;
        }

        /// <summary>
        /// Search for gas stations within a bounding box.
        /// </summary>
        /// <example>
        /// POST /api/gasstationgeo/search-bbox
        /// </example>
        [HttpPost("search-bounding-box")]
        public async Task<IActionResult> SearchBoundingBox([FromBody] GeoBoundingBox box)
        {
            if (box == null)
                return BadRequest("Bounding box data is required.");

            var results = await _geoSearch.SearchBaseBoundingBox(box);
            return Ok(results);
        }

        /// <summary>
        /// Find the nearest gas station to a given coordinate.
        /// </summary>
        /// <example>
        /// POST /api/gasstationgeo/nearest
        /// </example>
        [HttpPost("find-nearest-gas-station")]
        public async Task<IActionResult> FindNearest([FromBody] Coordinate coordinate)
        {
            if (coordinate == null)
                return BadRequest("Coordinate data is required.");

            var result = await _geoSearch.FindNearestGeoModel(coordinate);
            if (result == null)
                return NotFound("No nearby gas stations found.");

            return Ok(result);
        }

        /// <summary>
        /// Bulk insert or update gas station geo data.
        /// </summary>
        /// <example>
        /// POST /api/gasstationgeo/set
        /// </example>
        [HttpPost("set")]
        public async Task<IActionResult> SetGeoData([FromBody] List<GasStation> stations)
        {
            if (stations == null || stations.Count == 0)
                return BadRequest("No gas station data provided.");

            await _geoSearch.SetGeoData(stations);
            return Ok(new { Message = $"{stations.Count} gas stations added/updated successfully." });
        }
    }

}
