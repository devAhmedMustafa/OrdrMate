using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;
using OrdrMate.Repositories;

public class ItemRepo(OrdrMateDbContext context, ItemCacheRepo cache) : IItemRepo
{
    private readonly OrdrMateDbContext _context = context;
    private readonly ItemCacheRepo _cache = cache;

    public async Task<Item?> AddItem(Item item)
    {
        try
        {
            var Pharmacy = await _context.Pharmacy
                .FirstOrDefaultAsync(r => r.Id == item.PharmacyId);

            if (Pharmacy == null)
            {
                Console.Error.WriteLine($"Pharmacy with ID {item.PharmacyId} not found.");
                throw new Exception("Pharmacy not found");
            }

            await _context.Item.AddAsync(item);
            await _context.SaveChangesAsync();

            return item;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding item: {ex.Message}");
            throw new Exception("Failed to add item");
        }
    }

    public async Task<Item?> GetItem(string id)
    {
        var item = await _context.Item
            .FirstOrDefaultAsync(i => i.Id == id);

        return item;
    }

    public async Task<IEnumerable<Item>> GetAllItems()
    {
        return await _context.Item.ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsByPharmacyId(string pharmacyId)
    {
        return await _context.Item
            .Where(i => i.PharmacyId == pharmacyId)
            .ToListAsync();
    }

    public async Task<Item?> UpdateItem(string id, Item item)
    {
        var existingItem = await _context.Item.FindAsync(id);
        if (existingItem == null)
        {
            throw new Exception("Item not found");
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.ImageUrl = item.ImageUrl;
        existingItem.Price = item.Price;
        existingItem.Category = item.Category;
        existingItem.Priority = item.Priority;
        existingItem.Tags = item.Tags;

        await _context.SaveChangesAsync();
        return existingItem;
    }

    public async Task<Item?> UpdateItem(Item item)
    {
        var entity = _context.Item.Update(item);
        await _context.SaveChangesAsync();
        return entity.Entity;
    }

    public async Task<bool> DeleteItem(string id)
    {
        var item = await _context.Item.FindAsync(id);
        if (item == null)
        {
            return false;
        }

        _context.Item.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Item>> GetAvailableItemsByBranchId(string branchId)
    {
        var branch = await _context.Branch
        .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null) throw new Exception("Branch not found");

        var items = await _context.Item
            .Where(i => i.PharmacyId == branch.PharmacyId)
            .ToListAsync();

        return items;
    }

    public async Task<IEnumerable<Item>> GetItemsByCategory(string pharmacyId, string category)
    {
        return await _context.Item
            .Where(i => i.PharmacyId == pharmacyId && i.Category == category)
            .ToListAsync();
    }
}