
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Riders.GeoLocation;

public class RiderGeoService
{
    private readonly RiderGeoRepo _riderGeoRepo;

    public RiderGeoService(RiderGeoRepo riderGeoRepo)
    {
        _riderGeoRepo = riderGeoRepo;
    }

    public async Task UpdateRiderLocationAsync(string riderId, GeoLocationDto geoData)
    {

        var existingData = await _riderGeoRepo.GetAsync(riderId);

        var riderMetadata = new RiderMetadata
        {
            Latitude = geoData.Latitude,
            Longitude = geoData.Longitude,
            IsAvailable = existingData?.IsAvailable ?? true
        };

        await _riderGeoRepo.SetAsync(riderId, riderMetadata);
    }

    public async Task<string> GetClosestAvailableRider(float latitude, float longitude)
    {
        try
        {
            var nearestRider = await _riderGeoRepo.GetNearestAvailableRiderAsync(latitude, longitude);
            if (nearestRider == null)
            {
                throw new NotFoundException("No available riders found nearby.");
            }

            return nearestRider;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding closest rider: {ex.Message}");
        }
    }

}