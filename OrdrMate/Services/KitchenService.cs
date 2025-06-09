using OrdrMate.DTOs.Kitchen;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

using OrdrMate.Events;
using OrdrMate.Models;

public class KitchenService
{
    private readonly IKitchenRepo _kitchenRepo;

    public KitchenService(IKitchenRepo kitchenRepo)
    {
        _kitchenRepo = kitchenRepo;
    }

    public async Task<KitchenDto?> AddKitchen(AddKitchenDto kitchenDto)
    {
        var kitchen = new Kitchen
        {
            Id = Guid.NewGuid().ToString(),
            Name = kitchenDto.Name,
            Description = kitchenDto.Description,
            RestaurantId = kitchenDto.RestaurantId
        };

        var createdKitchen = await _kitchenRepo.CreateKitchen(kitchen);
        if (createdKitchen == null)
        {
            throw new InvalidOperationException("Failed to create kitchen.");
        }

        return new KitchenDto
        {
            Id = createdKitchen.Id,
            Name = createdKitchen.Name,
            Description = createdKitchen.Description,
        };
    }

    public async Task<List<KitchenDto>> GetKitchensByRestaurantId(string restaurantId)
    {
        if (string.IsNullOrEmpty(restaurantId))
        {
            throw new ArgumentException("Restaurant ID cannot be null or empty.", nameof(restaurantId));
        }
        var kitchens = await _kitchenRepo.GetKitchensByRestaurantId(restaurantId);

        return [.. kitchens.Select(k => new KitchenDto
        {
            Id = k.Id,
            Name = k.Name,
            Description = k.Description,
        })];
    }

    public async Task<bool> DeleteKitchen(string kitchenId)
    {
        if (string.IsNullOrEmpty(kitchenId))
        {
            throw new ArgumentException("Kitchen ID cannot be null or empty.", nameof(kitchenId));
        }
        return await _kitchenRepo.DeleteKitchen(kitchenId);
    }

    public async Task<KitchenPowerDto?> AddKitchenPower(AddKitchenPowerDto kitchenPowerDto)
    {
        if (kitchenPowerDto == null)
        {
            throw new ArgumentNullException(nameof(kitchenPowerDto), "Kitchen power data cannot be null.");
        }

        var kitchenPower = new KitchenPower
        {
            BranchId = kitchenPowerDto.BranchId,
            KitchenId = kitchenPowerDto.KitchenId,
            Units = kitchenPowerDto.Units,
        };

        var createdKitchenPower = await _kitchenRepo.CreateKitchenPower(kitchenPower);
        if (createdKitchenPower == null)
        {
            throw new InvalidOperationException("Failed to create kitchen power.");
        }
        return new KitchenPowerDto
        {
            BranchId = createdKitchenPower.BranchId,
            KitchenId = createdKitchenPower.KitchenId,
            KitchenName = createdKitchenPower.Kitchen?.Name ?? "Unknown Kitchen",
            Units = createdKitchenPower.Units,
        };
    }

    public async Task<KitchenPowerDto?> GetKitchenPower(string branchId, string kitchenId)
    {
        if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(kitchenId))
        {
            throw new ArgumentException("Branch ID and Kitchen ID cannot be null or empty.");
        }
        var kitchenPower = await _kitchenRepo.GetKitchenPowerByBranchIdAndKitchenId(branchId, kitchenId);

        if (kitchenPower == null)
        {
            return null;
        }

        return new KitchenPowerDto
        {
            BranchId = kitchenPower.BranchId,
            KitchenId = kitchenPower.KitchenId,
            KitchenName = kitchenPower.Kitchen?.Name ?? "Unknown Kitchen",
            Units = kitchenPower.Units,
        };
    }

    public async Task<KitchenDto?> GetKitchenById(string kitchenId)
    {
        if (string.IsNullOrEmpty(kitchenId))
        {
            throw new ArgumentException("Kitchen ID cannot be null or empty.", nameof(kitchenId));
        }
        var kitchen = await _kitchenRepo.GetKitchenById(kitchenId);
        if (kitchen == null)
        {
            return null;
        }
        return new KitchenDto
        {
            Id = kitchen.Id,
            Name = kitchen.Name,
            Description = kitchen.Description,
            RestaurantId = kitchen.RestaurantId
        };
    }

    public async Task<KitchenPowerDto?> UpdateKitchenPower(string branchId, string kitchenId, UpdateKitchenPowerDto kitchenPowerDto)
    {
        var kitchenPower = await _kitchenRepo.GetKitchenPowerByBranchIdAndKitchenId(branchId, kitchenId);
        if (kitchenPower == null)
        {
            throw new InvalidOperationException($"Kitchen power not found for branch {branchId} and kitchen {kitchenId}.");
        }
        kitchenPower.Units = kitchenPowerDto.Units;
        kitchenPower.Status = kitchenPowerDto.Status;

        var updatedKitchenPower = await _kitchenRepo.UpdateKitchenPower(branchId, kitchenId, kitchenPower);

        if (updatedKitchenPower == null) return null;

        BranchEvents.OnKitchenUpdate(branchId, updatedKitchenPower.Kitchen!.Name, updatedKitchenPower.Units);

        return new KitchenPowerDto
        {
            BranchId = updatedKitchenPower.BranchId,
            KitchenId = updatedKitchenPower.KitchenId,
            KitchenName = updatedKitchenPower.Kitchen?.Name ?? "Unknown Kitchen",
            Units = updatedKitchenPower.Units,
            Status = updatedKitchenPower.Status
        };
    }
            
    
}