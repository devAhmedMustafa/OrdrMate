namespace OrdrMate.Repositories;

using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

public class BranchRequestRepo : IBranchRequestRepo
{
    private readonly OrdrMateDbContext _context;

    public BranchRequestRepo(OrdrMateDbContext context)
    {
        _context = context;
    }

    public async Task<BranchRequest> GetBranchRequestById(string id)
    {
        return await _context.BranchRequest.FindAsync(id) ?? throw new KeyNotFoundException($"BranchRequest with id {id} not found.");
    }

    public async Task<IEnumerable<BranchRequest>> GetAllBranchRequests()
    {
        return await _context.BranchRequest.Include(b => b.Restaurant).ToListAsync();
    }

    public async Task<BranchRequest> CreateBranchRequest(BranchRequest branchRequest)
    {
        await _context.BranchRequest.AddAsync(branchRequest);
        await _context.SaveChangesAsync();
        return branchRequest;
    }

    public async Task<bool> DeleteBranchRequest(string id)
    {
        var branchRequest = await GetBranchRequestById(id);
        if (branchRequest == null)
        {
            return false;
        }

        _context.BranchRequest.Remove(branchRequest);
        await _context.SaveChangesAsync();
        return true;
    }
}