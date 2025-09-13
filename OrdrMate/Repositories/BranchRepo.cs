namespace OrdrMate.Repositories;

using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

public class BranchRepo : IBranchRepo
{
    private readonly OrdrMateDbContext _context;

    public BranchRepo(OrdrMateDbContext context)
    {
        _context = context;
    }

    public async Task<Branch> GetBranchById(string id)
    {
        return await _context.Branch.Include(b => b.Restaurant)
        .FirstOrDefaultAsync(b => b.Id == id)
        ?? throw new KeyNotFoundException($"Branch with id {id} not found.");
    }

    public async Task<Branch> GetDetailedBranchById(string id)
    {
        return await _context.Branch
        .Include(b => b.KitchenPowers)!.ThenInclude(kp => kp.Kitchen)
        .Include(b => b.Orders!.OrderBy(o => o.OrderDate)).ThenInclude(o => o.OrderItems)!.ThenInclude(oi => oi.Item).ThenInclude(i => i.Kitchen)
        .Include(b => b.Tables)
        .AsSplitQuery()
        .AsNoTracking()
        .FirstOrDefaultAsync(b => b.Id == id)
        ?? throw new KeyNotFoundException($"Branch with id {id} not found.");
    }

    public async Task<IEnumerable<Branch>> GetAllBranches()
    {
        return await _context.Branch
        .Include(b => b.Restaurant).ThenInclude(r => r!.Profile)
        .Include(b => b.Tables)
        .Include(b => b.Orders!.OrderBy(o => o.OrderDate)).ThenInclude(o => o.OrderItems)!.ThenInclude(oi => oi.Item).ThenInclude(i => i.Kitchen)
        .Include(b => b.KitchenPowers)!.ThenInclude(kp => kp.Kitchen)
        .AsSplitQuery()
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<IEnumerable<Branch>> GetRestaurantBranches(string restaurantId)
    {
        return await _context.Branch
            .Where(b => b.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<Branch> GetBranchByManagerId(string managerId)
    {
        return await _context.Branch
            .FirstOrDefaultAsync(b => b.BranchManagerId == managerId)
            ?? throw new KeyNotFoundException($"Branch with manager id {managerId} not found.");
    }

    public async Task<Branch> CreateBranch(Branch branch)
    {
        await _context.Branch.AddAsync(branch);
        await _context.SaveChangesAsync();

        var restaurant = await _context.Restaurant
            .Include(r => r.Kitchens)
            .FirstOrDefaultAsync(r => r.Id == branch.RestaurantId) ?? throw new KeyNotFoundException($"Restaurant with id {branch.RestaurantId} not found.");

        if (restaurant.Kitchens == null)
        {
            throw new InvalidOperationException($"Restaurant with id {branch.RestaurantId} has no kitchens defined.");
        }

        foreach (var kitchen in restaurant.Kitchens)
        {
            await _context.KitchenPower.AddAsync(new KitchenPower
            {
                KitchenId = kitchen.Id,
                BranchId = branch.Id
            });
        }

        return branch;
    }

    public async Task<bool> DeleteBranch(string id)
    {
        var branch = await GetBranchById(id);
        if (branch == null)
        {
            return false;
        }

        _context.Branch.Remove(branch);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateBranch(Branch branch)
    {
        _context.Branch.Update(branch);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BranchExists(string id)
    {
        return await _context.Branch.AnyAsync(b => b.Id == id);
    }

    public async Task<bool> HasAccess(string branchId, string managerId)
    {
        var branch = await _context.Branch
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
        {
            return false;
        }

        if (branch.BranchManagerId == managerId) return true;

        var restaurant = await _context.Restaurant
            .Include(r => r.Branches)
            .FirstOrDefaultAsync(r => r.Id == branch.RestaurantId);

        return restaurant?.ManagerId == managerId;
    }

    public async Task<int> GetTableCount(string branchId)
    {
        return await _context.Table
            .Where(t => t.BranchId == branchId)
            .CountAsync();
    }

    public async Task<int> GetAvailableTables(string branchId)
    {

        var reservations = await _context.TableReservation
            .Where(r => r.BranchId == branchId && (r.ReservationStatus == "Queued" || r.ReservationStatus == "Seated"))
            .ToListAsync();

        var reservedTables = reservations
            .Select(r => r.TableNumber)
            .Distinct()
            .ToList();

        var totalTables = await _context.Table
            .Where(t => t.BranchId == branchId)
            .CountAsync();
            
        return totalTables - reservedTables.Count;
    }

    public async Task<int> GetOrdersInQueue(string branchId)
    {
        var orders = await _context.Order
            .Where(o => o.BranchId == branchId && o.Status == Enums.OrderStatus.Queued)
            .CountAsync();
        return orders;
    }

}