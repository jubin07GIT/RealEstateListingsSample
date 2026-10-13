using Listings.Api.Data;
using Listings.Api.Models;
using Listings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Listings.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly AccessRepository _db;
    public ListingsController(AccessRepository db) => _db = db;

    /// <summary>Search source MLS listing rows.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Listing>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Listing>>> GetAll([FromQuery] ListingFilters filters)
    {
        var records = await _db.GetAllAsync();
        return Ok(records.Where(filters.Matches).OrderByDescending(x => x.ListedDate).ToList());
    }

    /// <summary>Find one source listing by MLS listing ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Listing), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Listing>> GetById(string id)
    {
        var records = await _db.GetAllAsync();
        var listing = records.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return listing is null ? NotFound() : Ok(listing);
    }
}
