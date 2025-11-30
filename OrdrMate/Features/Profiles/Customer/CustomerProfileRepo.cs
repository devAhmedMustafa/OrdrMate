using OrdrMate.Data;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Profiles.Customer;

public class CustomerProfileRepo
{
    private readonly OrdrMateDbContext _db;

    public CustomerProfileRepo(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task CreateProfile(CustomerProfile profile)
    {
        try
        {
            _db.CustomerProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to create customer profile: {ex.Message}");
        }
    }
}