using System.Data.OleDb;
using Dapper;
using Listings.Api.Models;

namespace Listings.Api.Data;

public sealed class AccessRepository(IConfiguration configuration, IWebHostEnvironment environment)
{
    private string DatabasePath
    {
        get
        {
            var configured = configuration["Access:DatabasePath"] ?? "../../Database/Listings.accdb";
            return Path.IsPathRooted(configured) ? configured
                : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));
        }
    }

    private OleDbConnection OpenConnection()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Access ACE OLE DB requires Windows.");
        if (!File.Exists(DatabasePath))
            throw new FileNotFoundException("Database missing: run Database/Initialize-Database.ps1 first.", DatabasePath);
        // Install the x64 Microsoft Access Database Engine or Microsoft Access (ACE provider).
        var connection = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DatabasePath};Persist Security Info=False;");
        connection.Open();
        return connection;
    }

    public async Task<IReadOnlyList<Listing>> GetAllAsync()
    {
        using var connection = OpenConnection();
        const string sql = "SELECT [Id], [Source], [Address], [City], [State], [Zip], [Price], " +
            "[Bedrooms], [Bathrooms], [Sqft], [Latitude], [Longitude], [ListedDate], [Status], [Description] FROM [Listings]";
        var results = await connection.QueryAsync<Listing>(sql);
        return results.ToList();
    }

    public int Import(IReadOnlyList<Listing> listings)
    {
        if (listings.Count == 0) throw new ArgumentException("No records to import.");
        if (listings.Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != listings.Count)
            throw new ArgumentException("Duplicate listing IDs in import.");
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            using (var delete = new OleDbCommand("DELETE FROM [Listings]", connection, transaction))
                delete.ExecuteNonQuery();
            const string insert = "INSERT INTO [Listings] ([Id],[Source],[Address],[City],[State],[Zip],[Price]," +
                "[Bedrooms],[Bathrooms],[Sqft],[Latitude],[Longitude],[ListedDate],[Status],[Description]) " +
                "VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
            foreach (var listing in listings)
            {
                using var cmd = new OleDbCommand(insert, connection, transaction);
                // ACE positional placeholders: parameter order MUST equal column order.
                cmd.Parameters.Add("@p1", OleDbType.VarWChar).Value = listing.Id;
                cmd.Parameters.Add("@p2", OleDbType.VarWChar).Value = listing.Source;
                cmd.Parameters.Add("@p3", OleDbType.VarWChar).Value = listing.Address;
                cmd.Parameters.Add("@p4", OleDbType.VarWChar).Value = listing.City;
                cmd.Parameters.Add("@p5", OleDbType.VarWChar).Value = listing.State;
                cmd.Parameters.Add("@p6", OleDbType.VarWChar).Value = listing.Zip;
                cmd.Parameters.Add("@p7", OleDbType.Currency).Value = listing.Price;
                cmd.Parameters.Add("@p8", OleDbType.Integer).Value = listing.Bedrooms;
                cmd.Parameters.Add("@p9", OleDbType.Double).Value = (double)listing.Bathrooms;
                cmd.Parameters.Add("@p10", OleDbType.Integer).Value = listing.Sqft;
                cmd.Parameters.Add("@p11", OleDbType.Double).Value = listing.Latitude;
                cmd.Parameters.Add("@p12", OleDbType.Double).Value = listing.Longitude;
                cmd.Parameters.Add("@p13", OleDbType.Date).Value = listing.ListedDate;
                cmd.Parameters.Add("@p14", OleDbType.VarWChar).Value = listing.Status;
                cmd.Parameters.Add("@p15", OleDbType.LongVarWChar).Value = listing.Description;
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
            return listings.Count;
        }
        catch { transaction.Rollback(); throw; }
    }
}
