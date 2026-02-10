namespace DataSyncService.Configuration;

/// <summary>
/// Configuration for database connection strings per region
/// </summary>
public class RegionConfiguration
{
    public const string ConfigSection = "RegionConnections";

    public string? EUConnectionString { get; set; }
    public string? EUMainConnectionString { get; set; }
    public string? USConnectionString { get; set; }
    public string? INConnectionString { get; set; }

    public string? GetConnectionString(string region)
    {
        return region.ToUpperInvariant() switch
        {
            "EU" => EUConnectionString,
            "EU-MAIN" => EUMainConnectionString,
            "US" => USConnectionString,
            "IN" => INConnectionString,
            _ => null
        };
    }

    public Dictionary<string, string> GetAllRegions()
    {
        var regions = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(EUConnectionString))
            regions["EU"] = EUConnectionString;
        if (!string.IsNullOrEmpty(EUMainConnectionString))
            regions["EU-MAIN"] = EUMainConnectionString;
        if (!string.IsNullOrEmpty(USConnectionString))
            regions["US"] = USConnectionString;
        if (!string.IsNullOrEmpty(INConnectionString))
            regions["IN"] = INConnectionString;

        return regions;
    }
}
