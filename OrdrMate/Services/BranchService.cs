namespace OrdrMate.Services;

using OrdrMate.DTOs.Branch;
using OrdrMate.DTOs.User;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils;
using OrdrMate.Events;
using OrdrMate.Managers;

public class BranchService(
    IBranchRepo branchRepo,
    ManagerService managerService,
    RestaurantService restaurantService,
    OrderManager orderManager,
    OrderService orderService
    )
{
    private readonly IBranchRepo _branchRepo = branchRepo;
    private readonly ManagerService _managerService = managerService;
    private readonly OrderManager _orderManager = orderManager;
    private readonly RestaurantService _restaurantService = restaurantService;
    private readonly OrderService _orderService = orderService;

    public async Task<BranchDto> GetBranchById(string id)
    {
        var branch = await _branchRepo.GetBranchById(id);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with id {id} not found.");
        }
        return new BranchDto
        {
            BranchId = branch.Id,
            Latitude = branch.Lantitude,
            Longitude = branch.Longitude,
            BranchAddress = branch.Address,
            RestaurantId = branch.RestaurantId,
        };
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranches()
    {
        var branches = await _branchRepo.GetAllBranches();
        return branches.Select(b => new BranchDto
        {
            BranchId = b.Id,
            Latitude = b.Lantitude,
            Longitude = b.Longitude,
            BranchAddress = b.Address,
            RestaurantId = b.RestaurantId,
            RestaurantName = b.Restaurant?.Name ?? "Unknown",
        });
    }

    public async Task<IEnumerable<BranchDto>> GetRestaurantBranches(string restaurantId)
    {
        var branches = await _branchRepo.GetRestaurantBranches(restaurantId);
        return branches.Select(b => new BranchDto
        {
            BranchId = b.Id,
            Latitude = b.Lantitude,
            Longitude = b.Longitude,
            BranchAddress = b.Address,
            RestaurantId = b.RestaurantId,
        });
    }

    public async Task<BranchApprovalDto> CreateBranch(BranchDto branchDto)
    {

        var restaurant = await _restaurantService.GetRestaurantById(branchDto.RestaurantId);

        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with id {branchDto.RestaurantId} not found.");
        }

        var username = RandomGenerator.GenerateRandomString(9, restaurant.Name);
        var password = RandomGenerator.GenerateRandomPassword(8);

        var createdManager = await _managerService.CreateManager(new CreateManagerDTO
        {
            Username = username,
            Password = password,
        });

        if (createdManager == null)
        {
            throw new Exception("Failed to create manager.");
        }

        var branch = new Branch
        {
            Id = Guid.NewGuid().ToString(),
            Lantitude = branchDto.Latitude,
            Longitude = branchDto.Longitude,
            Address = branchDto.BranchAddress,
            Phone = branchDto.BranchPhoneNumber,
            RestaurantId = branchDto.RestaurantId,
            BranchManagerId = createdManager.Id,
        };

        var createdBranch = await _branchRepo.CreateBranch(branch);

        BranchEvents.OnBranchCreated(createdBranch);

        return new BranchApprovalDto
        {
            BranchId = createdBranch.Id,
            BranchAddress = createdBranch.Address,
            RestaurantId = createdBranch.RestaurantId,
            BranchPhoneNumber = createdBranch.Phone,
            BranchManagerId = createdBranch.BranchManagerId,
            BranchManagerUsername = username,
            BranchManagerPassword = password,
        };
    }

    public async Task<BranchInfoDto> GetBranchInfo(string branchId)
    {
        var branch = await _branchRepo.GetBranchById(branchId);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with id {branchId} not found.");
        }

        var ordersInQueue = await _branchRepo.GetOrdersInQueue(branchId);
        var freeTables = await _branchRepo.GetFreeTables(branchId);
        var waitingTimes = await _orderManager.GetEstimatedTimes(branchId);

        return new BranchInfoDto
        {
            BranchId = branch.Id,
            BranchAddress = branch.Address,
            BranchPhoneNumber = branch.Phone,
            RestaurantName = branch.Restaurant?.Name ?? "Unknown",
            FreeTables = freeTables,
            OrdersInQueue = ordersInQueue,
            MinWaitingTime = waitingTimes.MinWaitingTime,
            MaxWaitingTime = waitingTimes.MaxWaitingTime,
            AverageWaitingTime = waitingTimes.AverageWaitingTime
        };
    }

    public async Task<BranchBalanceDto> GetBranchBalance(string branchId)
    {
        var paidOrders = await _orderService.GetPaidOrders(branchId);
        var todayEarnings = paidOrders
            .Where(o => o.OrderDate.Date == DateTime.UtcNow.Date)
            .Sum(o => o.TotalAmount);

        var totalEarnings = paidOrders.Sum(o => o.TotalAmount);
        
        return new BranchBalanceDto
        {
            TotalEarnings = totalEarnings,
            TodayEarnings = todayEarnings,
        };
    }

}