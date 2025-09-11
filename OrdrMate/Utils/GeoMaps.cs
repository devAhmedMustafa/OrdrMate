using Newtonsoft.Json.Linq;

namespace OrdrMate.Utils;

public class GeoMaps(IConfiguration configuration)
{

    private static readonly HttpClient httpClient = new();
    private readonly string _apiKey = configuration["GoogleMaps:ApiKey"]
    ?? throw new ArgumentNullException("GoogleMaps:ApiKey is not configured");

    public async Task<double> CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={lat1},{lon1}&destinations={lat2},{lon2}&key={_apiKey}";

        var response = await httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        var json = JObject.Parse(content);

        var status = json["status"]?.ToString();
        if (status != "OK")
            throw new Exception($"API returned status: {status}");

        if (json["rows"] is not JArray rows || rows.Count == 0)
            throw new Exception("No rows returned from API");

        var elements = rows[0]["elements"] as JArray;

        if (elements == null || elements.Count == 0)
            throw new Exception("No elements returned from API");

        var distanceValue = elements[0]["distance"]?["value"]?.ToObject<double>();

        if (distanceValue == null)
            throw new Exception("Distance not available");

        return distanceValue.Value / 1000.0;
    }

}