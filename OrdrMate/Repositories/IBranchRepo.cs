namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface IBranchRepo
{
    Task<Branch> GetBranchById(string id);
    Task<Branch> GetBranchByManagerId(string managerId);
    Task<IEnumerable<Branch>> GetAllBranches();
    Task<IEnumerable<Branch>> GetRestaurantBranches(string restaurantId);
    Task<Branch> CreateBranch(Branch branch);
    Task<bool> DeleteBranch(string id);
    Task<bool> UpdateBranch(Branch branch);
    Task<bool> BranchExists(string id);
    Task<bool> HasAccess(string branchId, string managerId);
    Task<Branch> GetDetailedBranchById(string id);
    Task<int> GetTableCount(string branchId);
    Task<int> GetAvailableTables(string branchId);
    Task<int> GetOrdersInQueue(string branchId);
}