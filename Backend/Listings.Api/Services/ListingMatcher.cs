using System.Globalization;
using System.Text.RegularExpressions;
using Listings.Api.Models;

namespace Listings.Api.Services;

public static partial class ListingMatcher
{
    // Conservative matching: identical coordinates (rounded to 4 decimals) with the same city/state,
    // OR exact normalized street address with the same city/state. ZIP is intentionally not a match gate,
    // so bad source ZIP data can be surfaced for review. This is a sample heuristic, not a full entity-resolution system.
    public static IReadOnlyList<ListingGroup> Group(IReadOnlyList<Listing> listings)
    {
        var parents = Enumerable.Range(0, listings.Count).ToArray();
        int Find(int x) { while (parents[x] != x) { parents[x] = parents[parents[x]]; x = parents[x]; } return x; }
        void Join(int a, int b) { a = Find(a); b = Find(b); if (a != b) parents[b] = a; }
        for (int a = 0; a < listings.Count; a++)
        for (int b = a + 1; b < listings.Count; b++)
        {
            var x = listings[a]; var y = listings[b];
            if (!string.Equals(x.City, y.City, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(x.State, y.State, StringComparison.OrdinalIgnoreCase)) continue;
            var sameCoordinates = x.Latitude != 0 && x.Longitude != 0 && y.Latitude != 0 && y.Longitude != 0 &&
                Math.Round(x.Latitude, 4) == Math.Round(y.Latitude, 4) &&
                Math.Round(x.Longitude, 4) == Math.Round(y.Longitude, 4);
            var sameAddress = Normalize(x.Address) == Normalize(y.Address);
            if (sameCoordinates || sameAddress) Join(a, b);
        }
        return listings.Select((item, index) => (item, index))
            .GroupBy(x => Find(x.index))
            .Select(g =>
            {
                var records = g.Select(x => x.item).OrderByDescending(x => x.ListedDate).ThenBy(x => x.Id).ToList();
                var preferred = records[0];
                return new ListingGroup(preferred.Id, preferred, records,
                    records.Select(x => x.Zip).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1,
                    records.Select(x => x.Price).Distinct().Count() > 1);
            })
            .OrderByDescending(g => g.Preferred.ListedDate).ToList();
    }

    public static string Normalize(string address)
    {
        var value = Regex.Replace(address.Trim().ToUpperInvariant(), @"[^A-Z0-9]+", " ");
        var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t switch {
                "STREET" or "ST" => "ST", "AVENUE" or "AVE" => "AVE", "ROAD" or "RD" => "RD",
                "LANE" or "LN" => "LN", "COURT" or "CT" => "CT", "DRIVE" or "DR" => "DR",
                "BOULEVARD" or "BLVD" => "BLVD", "APARTMENT" or "APT" or "UNIT" => "UNIT",
                _ => t });
        return string.Join(' ', tokens);
    }
}
