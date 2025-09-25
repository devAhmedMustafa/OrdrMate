using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityRepository
{
    private readonly OrdrMateDbContext _db;

    public ItemAvailabilityRepository(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task<ItemAvailability> AddItemAvailability(ItemAvailability itemAvailability)
    {
        var entity = await _db.ItemAvailabilities.AddAsync(itemAvailability);
        await _db.SaveChangesAsync();
        return entity.Entity;
    }

    public async Task<bool> IsItemAvailable(string itemId, string branchId)
    {
        return await _db.ItemAvailabilities.AnyAsync(ia => ia.ItemId == itemId && ia.BranchId == branchId && ia.AvailableQuantity > 0);
    }

    public async Task<ItemAvailability?> GetItemAvailability(string itemId, string branchId)
    {
        return await _db.ItemAvailabilities.FirstOrDefaultAsync(ia => ia.ItemId == itemId && ia.BranchId == branchId);
    }

    public async Task<List<ItemAvailability>> GetAllItemAvailabilities(string branchId)
    {
        var itemAvailabilities = await _db.ItemAvailabilities
            .Where(ia => ia.BranchId == branchId)
            .ToListAsync();

        return itemAvailabilities;
    }

    public async Task<ItemAvailability> UpdateItemAvailability(ItemAvailability itemAvailability)
    {
        var entity = _db.ItemAvailabilities.Update(itemAvailability);
        await _db.SaveChangesAsync();
        return entity.Entity;
    }
    
    public async Task UpdateItemQuantity(string itemId, string branchId, int quantity)
    {
        var itemAvailability = await GetItemAvailability(itemId, branchId);
        if (itemAvailability is null)
        {
            throw new Exception("Item availability not found");
        }

        itemAvailability.AvailableQuantity = quantity;
        await UpdateItemAvailability(itemAvailability);
    }
}