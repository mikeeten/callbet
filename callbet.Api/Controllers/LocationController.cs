using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.Interfaces;

namespace callbet.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/location")]
    public class LocationController(ICustomerService customerService) : ControllerBase
    {
        // 1. Get all SubCities in Addis Ababa (with nested neighborhoods)
        [HttpGet("subcities")]
        public async Task<IActionResult> GetSubCities(CancellationToken ct)
        {
            var subCities = await customerService.GetSubCitiesAsync(ct);
            return Ok(subCities);
        }

        // 2. Get Neighborhoods filtered by SubCity ID
        [HttpGet("subcities/{subCityId}/neighborhoods")]
        [HttpGet("neighborhoods/by-subcity/{subCityId}")]
        public async Task<IActionResult> GetNeighborhoodsBySubCity(int subCityId, CancellationToken ct)
        {
            var neighborhoods = await customerService.GetNeighborhoodsBySubCityAsync(subCityId, ct);
            return Ok(neighborhoods);
        }

        // 3. Get all Neighborhoods
        [HttpGet("neighborhoods")]
        public async Task<IActionResult> GetAllNeighborhoods(CancellationToken ct)
        {
            var neighborhoods = await customerService.GetNeighborhoodsAsync(ct);
            return Ok(neighborhoods);
        }

        // 4. Trigger Seeding of SubCities and Neighborhoods
        [HttpPost("seed")]
        public async Task<IActionResult> SeedLocations([FromServices] IServiceProvider serviceProvider)
        {
            await callbet.Infrastructure.Persistence.DbInitializer.SeedLocationDataAsync(serviceProvider);
            return Ok(new { message = "Addis Ababa SubCities and Neighborhoods seeded successfully." });
        }
    }
}
