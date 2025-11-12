using Geo_Search.Interface;
using Geo_Search.Models;
using Microsoft.AspNetCore.Mvc;
using Mongo.Geo_Search;

public class PetShopGeoController : ControllerBase
{
    private readonly IGeoSearch<PetShopDto> _geoSearch;

    public PetShopGeoController(IGeoSearch<PetShopDto> geoSearch)
    {
        _geoSearch = geoSearch;
    }

    [HttpGet("nearest")]
    public async Task<IActionResult> GetNearest(double lat, double lng)
    {
        var nearest = await _geoSearch.FindNearestGeoModel(new Coordinate
        {
            Latitude = lat,
            Longitude = lng
        });
        return Ok(nearest);
    }

    [HttpPost("within")]
    public async Task<IActionResult> GetWithin([FromBody] GeoBoundingBox box)
    {
       
        var results = await _geoSearch.SearchBaseBoundingBox(box);
        return Ok(results);
    }
    /// <summary>
    /// Seeds sample PetShop data into MongoDB.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedData()
    {
        var sampleData = new List<PetShopDto>
            {
                new PetShopDto
                {
                    Name = "Happy Paws",
                    Coordinate = new Coordinate { Latitude = 40.7128, Longitude = -74.0060 } // NYC
                },
                new PetShopDto
                {
                    Name = "Puppy Palace",
                    Coordinate = new Coordinate { Latitude = 40.7138, Longitude = -74.0075 }
                },
                new PetShopDto
                {
                    Name = "Cat Corner",
                    Coordinate = new Coordinate { Latitude = 40.7145, Longitude = -74.0050 }
                },
                new PetShopDto
                {
                    Name = "Dog Depot",
                    Coordinate = new Coordinate { Latitude = 40.7100, Longitude = -74.0100 }
                }
            };

        await _geoSearch.SetGeoData(sampleData);

        return Ok(new
        {
            message = "PetShop data seeded successfully",
            count = sampleData.Count
        });
    }
}
