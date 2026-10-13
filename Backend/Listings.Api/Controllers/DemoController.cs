using System.Text.Json;
using Listings.Api.Data;
using Listings.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Listings.Api.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly AccessRepository _db;
    private readonly IWebHostEnvironment _environment;

    public DemoController(AccessRepository db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    /// <summary>Reload sample source records (development only; replaces existing rows).</summary>
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        if (!_environment.IsDevelopment()) return NotFound();
        var seedPath = Path.Combine(_environment.ContentRootPath, "Data", "sample_listings.json");
        if (!System.IO.File.Exists(seedPath))
            return Problem("Seed file not found.");
        var records = JsonSerializer.Deserialize<List<Listing>>(System.IO.File.ReadAllText(seedPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return Ok(new { imported = _db.Import(records ?? []) });
    }
}
