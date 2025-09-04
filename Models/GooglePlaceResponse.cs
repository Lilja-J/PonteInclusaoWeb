using System.Collections.Generic;
using System.Text.Json.Serialization;
public class GooglePlaceResponse
{
    [JsonPropertyName("results")]
    public List<GooglePlaceResult> Results { get; set; } = new();
}

public class GooglePlaceResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("formatted_address")]
    public string FormattedAddress { get; set; } = string.Empty;

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; } = new();

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("business_status")]
    public string BusinessStatus { get; set; } = string.Empty;
}

public class Geometry
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();
}

public class Location
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    
    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}