using Listings.Api.Data;
using Listings.Api.Models;
using Listings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Listings.Api.Controllers;

[ApiController]
[Route("api/stats")]
public sealed class StatsController : ControllerBase
{
    private readonly AccessRepository _db;
    public StatsController(AccessRepository db) => _db = db;

    [HttpGet]
    [ProducesResponseType(typeof(ListingStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListingStats>> Get()
    {
        var records = await _db.GetAllAsync();
        var groups = ListingMatcher.Group(records);
        return Ok(new ListingStats(records.Count, groups.Count, groups.Count(g => g.Records.Count > 1),
            groups.Count(g => g.HasConflictingZip),
            groups.Count(g => g.Preferred.Status.Equals("active", StringComparison.OrdinalIgnoreCase))));
    }
}
