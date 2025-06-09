namespace OrdrMate.Repositories;

using OrdrMate.Models;
using OrdrMate.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class KitchenRepo(OrdrMateDbContext context) : IKitchenRepo
{
    private readonly OrdrMateDbContext _db = context;

    public async Task<Kitchen?> CreateKitchen(Kitchen kitchen)
    {
        await _db.Kitchen.AddAsync(kitchen);

        var restaurant = await _db.Restaurant
            .Include(r => r.Branches)
            .FirstOrDefaultAsync(r => r.Id == kitchen.RestaurantId);

        var branches = restaurant?.Branches;

        Console.WriteLine($"Number of branches found: {branches?.Count ?? 0}");

        foreach (var branch in branches ?? [])
        {
            var kitchenPower = new KitchenPower
            {
                BranchId = branch.Id,
                KitchenId = kitchen.Id,
                Units = 0
            };

            await _db.KitchenPower.AddAsync(kitchenPower);
        }

        await _db.SaveChangesAsync();

        return kitchen;
    }

    public async Task<KitchenPower?> CreateKitchenPower(KitchenPower kitchenPower)
    {
        var kitchen = await _db.Kitchen.FindAsync(kitchenPower.KitchenId);
        if (kitchen == null)
        {
            throw new InvalidOperationException($"Kitchen with id {kitchenPower.KitchenId} does not exist.");
        }

        await _db.KitchenPower.AddAsync(kitchenPower);
        await _db.SaveChangesAsync();

        return kitchenPower;
    }

    public async Task<bool> DeleteKitchen(string kitchenId)
    {
        var kitchen = await _db.Kitchen.FindAsync(kitchenId);
        if (kitchen == null)
        {
            return false;
        }
        _db.Kitchen.Remove(kitchen);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Kitchen?> GetKitchenById(string kitchenId)
    {
        return await _db.Kitchen.FirstOrDefaultAsync(k => k.Id == kitchenId);
    }

    public async Task<KitchenPower?> GetKitchenPowerByBranchIdAndKitchenId(string branchId, string kitchenId)
    {
        return await _db.KitchenPower.Include(kp => kp.Kitchen)
            .FirstOrDefaultAsync(kp => kp.BranchId == branchId && kp.KitchenId == kitchenId);
    }

    public async Task<List<Kitchen>> GetKitchensByRestaurantId(string restaurantId)
    {
        return await _db.Kitchen
            .Where(k => k.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<Kitchen?> UpdateKitchen(string id, Kitchen kitchen)
    {
        throw new NotImplementedException();
    }

    public async Task<KitchenPower?> UpdateKitchenPower(string branchId, string kitchenId, KitchenPower kitchenPower)
    {
        var existingKitchenPower = await _db.KitchenPower
            .Include(kp => kp.Kitchen)
            .FirstOrDefaultAsync(kp => kp.BranchId == branchId && kp.KitchenId == kitchenId);
        if (existingKitchenPower == null)
        {
            throw new InvalidOperationException($"Kitchen power for branch {branchId} and kitchen {kitchenId} does not exist.");
        }
        existingKitchenPower.Units = kitchenPower.Units;
        await _db.SaveChangesAsync();
        return existingKitchenPower;
    }
    
}