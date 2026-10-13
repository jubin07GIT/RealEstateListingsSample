using Listings.Api.Models;
using Listings.Api.Services;
using Xunit;

public class ListingMatcherTests
{
    [Fact]
    public void EquivalentAddressAndConflictingZip_AreFlagged()
    {
        var left = new Listing { Id = "A", Address = "456 Oak Ave", City = "Springfield", State = "VA", Zip = "22150", Price = 525000 };
        var right = new Listing { Id = "B", Address = "456 Oak Avenue", City = "Springfield", State = "VA", Zip = "22151", Price = 527500 };
        var groups = ListingMatcher.Group([left, right]);
        Assert.Single(groups);
        Assert.True(groups[0].HasConflictingZip);
        Assert.True(groups[0].HasConflictingPrice);
    }
    [Fact]
    public void DifferentCities_DoNotMatch_DespiteCoordinates()
    {
        var a = new Listing { Id = "A", Address = "1 Road", City = "Fairfax", State = "VA", Latitude = 38.8, Longitude = -77.1 };
        var b = new Listing { Id = "B", Address = "2 Road", City = "Reston", State = "VA", Latitude = 38.8, Longitude = -77.1 };
        Assert.Equal(2, ListingMatcher.Group([a,b]).Count);
    }
}
