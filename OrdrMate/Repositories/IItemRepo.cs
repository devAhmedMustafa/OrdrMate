using OrdrMate.Models;

namespace OrdrMate.Repositories;

public interface IItemRepo
{
    Task<Item?> AddItem(Item item);
    Task<Item?> GetItem(string id);
    Task<IEnumerable<Item>> GetAllItems();
    Task<IEnumerable<Item>> GetItemsByStoreId(string storeId);
    Task<IEnumerable<Item>> GetAvailableItemsByBranchId(string branchId);
    Task<Item?> UpdateItem(string id, Item item);
    Task<Item?> UpdateItem(Item item);
    Task<bool> DeleteItem(string id);
    Task<IEnumerable<Item>> GetItemsByCategory(string storeId, string category);
}