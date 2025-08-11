using MongoDB.Driver;
using OrdrMate.Data;

namespace OrdrMate.Features.Customization;

public class UserCustomizationRepo
{
    private readonly OrdrMateMongoContext _context;
    public UserCustomizationRepo(OrdrMateMongoContext context)
    {
        _context = context;
    }

    public async Task<UserCustomization?> GetUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        return await _context.UserCustomizations
            .Find(uc => uc.UserId == userId && uc.ItemId == itemId)
            .FirstOrDefaultAsync();
    }

    public async Task<UserCustomization> CreateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));
        await _context.UserCustomizations.InsertOneAsync(userCustomization);
        return userCustomization;
    }

    public async Task<bool> UpdateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));

        var result = await _context.UserCustomizations
            .ReplaceOneAsync(uc => uc.UserId == userCustomization.UserId && uc.ItemId == userCustomization.ItemId, userCustomization);

        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        var result = await _context.UserCustomizations
            .DeleteOneAsync(uc => uc.UserId == userId && uc.ItemId == itemId);

        return result.IsAcknowledged && result.DeletedCount > 0;
    }  
}