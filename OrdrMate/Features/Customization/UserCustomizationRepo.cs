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

    public async Task<UserCustomization?> GetUserCustomization(string orderId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        return await _context.UserCustomizations
            .Find(uc => uc.OrderId == orderId && uc.ItemId == itemId)
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
            .ReplaceOneAsync(uc => uc.OrderId == userCustomization.OrderId && uc.ItemId == userCustomization.ItemId, userCustomization);

        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteUserCustomization(string orderId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        var result = await _context.UserCustomizations
            .DeleteOneAsync(uc => uc.OrderId == orderId && uc.ItemId == itemId);

        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<List<UserCustomization>> GetOrderCustomizationsAsync(string orderId)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));

            var customizations = await _context.UserCustomizations
                .Find(uc => uc.OrderId == orderId)
                .ToListAsync();

            if (customizations == null || !customizations.Any())
            {
                return [];
            }

            return customizations;
        }
        catch (Exception ex)
        {
            
        }
    }
}