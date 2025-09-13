namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface IBranchRequestRepo
{
    Task<BranchRequest> GetBranchRequestById(string id);
    Task<IEnumerable<BranchRequest>> GetAllBranchRequests();
    Task<BranchRequest> CreateBranchRequest(BranchRequest branchRequest);
    Task<bool> DeleteBranchRequest(string id);
}