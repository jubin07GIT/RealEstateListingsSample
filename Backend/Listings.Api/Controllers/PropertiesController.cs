using Listings.Api.Data;
using Listings.Api.Models;
using Listings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Listings.Api.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController : ControllerBase
{
    private readonly AccessRepository _db;
    public PropertiesController(AccessRepository db) => _db = db;

    /// <summary>Return potential duplicate groups, including underlying source records.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ListingGroup>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ListingGroup>>> GetAll(
        [FromQuery] ListingFilters filters, [FromQuery] bool duplicatesOnly = false)
    {
        if (filters.MinPrice.HasValue && filters.MaxPrice.HasValue && filters.MinPrice.Value > filters.MaxPrice.Value)
            return BadRequest(new { error = "Minimum price cannot be greater than maximum price." });
        var groups = ListingMatcher.Group(await _db.GetAllAsync());
        return Ok(groups.Where(g =>
            (!duplicatesOnly || g.Records.Count > 1) &&
            // Search across all source descriptions/addresses, other filters use preferred record.
            (string.IsNullOrWhiteSpace(filters.City) || g.Preferred.City.Equals(filters.City, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filters.Status) || g.Preferred.Status.Equals(filters.Status, StringComparison.OrdinalIgnoreCase)) &&
            (!filters.MinPrice.HasValue || g.Preferred.Price >= filters.MinPrice.Value) &&
            (!filters.MaxPrice.HasValue || g.Preferred.Price <= filters.MaxPrice.Value) &&
            (!filters.MinBedrooms.HasValue || g.Preferred.Bedrooms >= filters.MinBedrooms.Value) &&
            (string.IsNullOrWhiteSpace(filters.Search) || g.Records.Any(x =>
                x.Address.Contains(filters.Search, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(filters.Search, StringComparison.OrdinalIgnoreCase)))).ToList());
    }
}
