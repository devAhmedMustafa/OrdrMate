namespace OrdrMate.Features.Customization;

public class UserCustomizationService
{
    private readonly UserCustomizationRepo _repo;
    public UserCustomizationService(UserCustomizationRepo repo)
    {
        _repo = repo;
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
}