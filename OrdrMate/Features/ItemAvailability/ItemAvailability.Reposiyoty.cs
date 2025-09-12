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

    public async Task<ItemAvailability?> GetItemAvailabilityById(int id)
    {
        return await _db.ItemAvailabilities.FindAsync(id);
    }

    public async Task<List<ItemAvailability>> GetAllItemAvailabilities()
    {
        var itemAvailabilities = await _db.ItemAvailabilities.Include(i => i.Item).ToListAsync();
        return itemAvailabilities;
    }
}