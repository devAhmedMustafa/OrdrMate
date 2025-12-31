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

    public async Task<CustomerProfile?> GetProfileByCustomerId(Guid customerId)
    {
        try
        {
            return await _db.CustomerProfiles.FindAsync(customerId);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to retrieve customer profile: {ex.Message}");
        }
    }

    public async Task UpdateProfile(CustomerProfile profile)
    {
        try
        {
            _db.CustomerProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to update customer profile: {ex.Message}");
        }
    }
}