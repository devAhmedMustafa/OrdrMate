using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;
using OrdrMate.Repositories;

public class ItemRepo(OrdrMateDbContext context) : IItemRepo
{
    private readonly OrdrMateDbContext _context = context;

    public async Task<Item?> AddItem(Item item)
    {

        // Check if the Store exists
        var Store = await _context.Store
            .FirstOrDefaultAsync(r => r.Id == item.StoreId);
        if (Store == null)
        {
            Console.Error.WriteLine($"Store with ID {item.StoreId} not found.");
            throw new Exception("Store not found");
        }

        await _context.Item.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
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

    public async Task<IEnumerable<Item>> GetItemsByStoreId(string storeId)
    {
        return await _context.Item
            .Where(i => i.StoreId == storeId)
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
            .Where(i => i.StoreId == branch.StoreId)
            .ToListAsync();

        return items;
    }

    public async Task<IEnumerable<Item>> GetItemsByCategory(string storeId, string category)
    {
        return await _context.Item
            .Where(i => i.StoreId == storeId && i.Category == category)
            .ToListAsync();
    }
}