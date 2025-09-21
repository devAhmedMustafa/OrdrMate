using OrdrMate.DTOs.Order;
using OrdrMate.Services;

namespace OrdrMate.Features.Customization;

public class UserCustomizationService
{
    private readonly UserCustomizationRepo _repo;
    private readonly CustomizationService _customizationService;
    public UserCustomizationService(UserCustomizationRepo repo, CustomizationService customizationService)
    {
        _repo = repo;
        _customizationService = customizationService;
    }

    public async Task<UserCustomization?> GetUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        return await _repo.GetUserCustomization(userId, itemId);
    }

    public async Task<UserCustomization> CreateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));
        return await _repo.CreateUserCustomization(userCustomization);
    }

    public async Task<bool> UpdateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));
        return await _repo.UpdateUserCustomization(userCustomization);
    }

    public async Task<bool> DeleteUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        return await _repo.DeleteUserCustomization(userId, itemId);
    }

    public async Task<bool> ValidateUserCustomization(OrderItemDto orderItem)
    {
        ArgumentNullException.ThrowIfNull(orderItem, nameof(orderItem));

        if (orderItem.Customizations == null || !orderItem.Customizations.Any())
        {
            throw new ArgumentNullException(nameof(orderItem.Customizations), "Order item customizations not found.");
        }

        var itemCustomizations = await _customizationService.GetItemCustomizations(orderItem.ItemId);
        if (itemCustomizations.Count() == 0)
        {
            return true;
        }

        foreach (var customization in itemCustomizations)
        {
            if (!orderItem.Customizations.ContainsKey(customization.Name))
            {
                return false;
            }
        }

        return true;
    }
}