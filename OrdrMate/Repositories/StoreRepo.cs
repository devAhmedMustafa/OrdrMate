using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

namespace OrdrMate.Repositories;

public class StoreRepo(OrdrMateDbContext c) : IStoreRepo
{

    private readonly OrdrMateDbContext _db = c;
    public async Task<Store> CreateStore(Store Store)
    {
        _db.Store.Add(Store);

        var profile = new StoreProfile
        {
            StoreId = Store.Id,
            Description = "Welcome to our Store!",
            LogoUrl = "https://example.com/default-logo.png",
            CoverImageUrl = "https://example.com/default-cover.png"
        };

        _db.StoreProfile.Add(profile);

        await _db.SaveChangesAsync();
        return Store;
    }

    public async Task<Store?> GetStoreById(string id)
    {
        return await _db.Store.Include(r => r.Profile).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> HasAccessToStore(string managerId, string StoreId)
    {
        var Store = await _db.Store
            .FirstOrDefaultAsync(r => r.Id == StoreId);
        if (Store == null)
        {
            return false;
        }

        return Store.ManagerId == managerId;
    }

    public async Task<Store?> GetStoreByManagerId(string managerId)
    {
        return await _db.Store
            .FirstOrDefaultAsync(r => r.ManagerId == managerId);
    }

    public async Task<IEnumerable<Store>> GetAllStores()
    {
        return await _db.Store.Include(r => r.Profile).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetStoreCategories(string StoreId)
    {
        var categories = await _db.Item
            .Where(i => i.StoreId == StoreId)
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync();

        return categories;
    }

    public async Task<IEnumerable<string>> GetStoreMainCategories(string StoreId)
    {
        var categories = await _db.Item
            .Where(i => i.StoreId == StoreId)
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync();

        return categories;
    }

    public async Task<StoreProfile?> GetStoreProfile(string StoreId)
    {
        var profile = await _db.StoreProfile
            .FirstOrDefaultAsync(p => p.StoreId == StoreId);

        if (profile == null)
        {
            profile = new StoreProfile
            {
                StoreId = StoreId,
                Description = "Welcome to our Store!",
                LogoUrl = "https://example.com/default-logo.png",
                CoverImageUrl = "https://example.com/default-cover.png"
            };
            _db.StoreProfile.Add(profile);
            await _db.SaveChangesAsync();
        }

        return profile;
    }

    public async Task<StoreProfile?> UpdateStoreProfile(string StoreId, StoreProfile profile)
    {
        var existingProfile = await _db.StoreProfile
            .FirstOrDefaultAsync(p => p.StoreId == StoreId);

        if (existingProfile == null)
        {
            throw new InvalidOperationException($"Profile for Store {StoreId} does not exist.");
        }

        existingProfile.Description = profile.Description;
        existingProfile.LogoUrl = profile.LogoUrl;
        existingProfile.CoverImageUrl = profile.CoverImageUrl;

        _db.StoreProfile.Update(existingProfile);
        await _db.SaveChangesAsync();

        return existingProfile;
    }
}