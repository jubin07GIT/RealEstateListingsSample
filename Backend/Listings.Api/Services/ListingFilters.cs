using Listings.Api.Models;

namespace Listings.Api.Services;

public sealed class ListingFilters
{
    public string? City { get; set; }
    public string? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public string? Search { get; set; }

    public bool Matches(Listing x) =>
        (string.IsNullOrWhiteSpace(City) || x.City.Equals(City, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(Status) || x.Status.Equals(Status, StringComparison.OrdinalIgnoreCase)) &&
        (!MinPrice.HasValue || x.Price >= MinPrice.Value) &&
        (!MaxPrice.HasValue || x.Price <= MaxPrice.Value) &&
        (!MinBedrooms.HasValue || x.Bedrooms >= MinBedrooms.Value) &&
        (string.IsNullOrWhiteSpace(Search) || x.Address.Contains(Search, StringComparison.OrdinalIgnoreCase)
            || x.Description.Contains(Search, StringComparison.OrdinalIgnoreCase));
}
