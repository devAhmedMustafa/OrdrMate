using OrdrMate.Models;

namespace OrdrMate.Repositories;

public interface IStoreRepo
{
    Task<Store> CreateStore(Store store);
    Task<Store?> GetStoreById(string id);
    Task<bool> HasAccessToStore(string managerId, string storeId);

    Task<Store?> GetStoreByManagerId(string managerId);
    Task<IEnumerable<Store>> GetAllStores();
    Task<IEnumerable<string>> GetStoreCategories(string storeId);
    Task<IEnumerable<string>> GetStoreMainCategories(string storeId);
    Task<StoreProfile?> GetStoreProfile(string storeId);
    Task<StoreProfile?> UpdateStoreProfile(string storeId, StoreProfile profile);
}