using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;
using StackExchange.Redis;

namespace OrdrMate.Features.Riders.GeoLocation;

public class RiderGeoRepo(IConnectionMultiplexer redis) : CacheRepo<RiderMetadata>(redis)
{
    private readonly string _geoKey = "riders:geo";

    public new async Task SetAsync(string riderId, RiderMetadata value, TimeSpan expiry = default)
    {
        try
        {
            await _cache.GeoAddAsync(_geoKey, value.Longitude, value.Latitude, riderId);
            await base.SetAsync(riderId, value, expiry);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error setting rider geo data: {ex.Message}");
        }
    }

    public async Task<string?> GetNearestAvailableRiderAsync(float latitude, float longitude)
    {
        try
        {
            var nearbyRiders = await _cache.GeoRadiusAsync(_geoKey, longitude, latitude, 5000, GeoUnit.Meters, 10, Order.Ascending);

            foreach (var rider in nearbyRiders)
            {
                var riderId = rider.Member.ToString();
                var metadata = await GetAsync(riderId);
                if (metadata != null && metadata.IsAvailable)
                {
                    return riderId;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error retrieving nearest available rider: {ex.Message}");
        }
    }

}