namespace Listings.Api.Models;

public sealed class Listing
{
    public string Id { get; set; } = "";
    public string Source { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public decimal Bathrooms { get; set; }
    public int Sqft { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime ListedDate { get; set; }
    public string Status { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed record ListingGroup(string GroupId, Listing Preferred, IReadOnlyList<Listing> Records,
    bool HasConflictingZip, bool HasConflictingPrice);

public sealed record ListingStats(int TotalRecords, int UniqueProperties, int MatchedGroups,
    int ConflictingZipGroups, int ActiveProperties);
