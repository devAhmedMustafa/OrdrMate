namespace OrdrMate.Services;

using OrdrMate.DTOs.Branch;
using OrdrMate.DTOs.User;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils;
using OrdrMate.Events;

public class BranchService(
    IBranchRepo branchRepo,
    ManagerService managerService,
    StoreService StoreService,
    OrderService orderService
    )
{
    private readonly IBranchRepo _branchRepo = branchRepo;
    private readonly ManagerService _managerService = managerService;
    private readonly StoreService _StoreService = StoreService;
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
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            BranchAddress = branch.Address,
            StoreId = branch.StoreId,
            StoreName = branch.Store?.Name ?? "Unknown",
            StartWorkingHour = branch.StartWorkingHour,
            EndWorkingHour = branch.EndWorkingHour,
            BranchPhoneNumber = branch.Phone,
            WorkingDays = branch.WorkingDays
        };
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranches()
    {
        var branches = await _branchRepo.GetAllBranches();
        return branches.Select(b => new BranchDto
        {
            BranchId = b.Id,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            BranchAddress = b.Address,
            StoreId = b.StoreId,
            StoreName = b.Store?.Name ?? "Unknown",
            IsOpen = TimeService.CheckWithinTimeInterval(b.StartWorkingHour, b.EndWorkingHour, b.WorkingDays),
            BranchPhoneNumber = b.Phone,
            LogoUrl = b.Store?.Profile?.LogoUrl,
            StartWorkingHour = b.StartWorkingHour,
            EndWorkingHour = b.EndWorkingHour,
            WorkingDays = b.WorkingDays
        });
    }

    public async Task<IEnumerable<BranchDto>> GetStoreBranches(string StoreId)
    {
        var branches = await _branchRepo.GetStoreBranches(StoreId);
        return branches.Select(b => new BranchDto
        {
            BranchId = b.Id,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            BranchAddress = b.Address,
            BranchPhoneNumber = b.Phone,
            StoreId = b.StoreId,
            StoreName = b.Store?.Name ?? "Unknown",
            StartWorkingHour = b.StartWorkingHour,
            EndWorkingHour = b.EndWorkingHour,
            WorkingDays = b.WorkingDays,
        });
    }

    public async Task<BranchApprovalDto> CreateBranch(CreateBranchDto branchDto)
    {
        try
        {
            var Store = await _StoreService.GetStoreById(branchDto.StoreId);

            if (Store == null) throw new KeyNotFoundException($"Store with id {branchDto.StoreId} not found.");


            var username = RandomGenerator.GenerateRandomString(Store.Name.Length + 4, Store.Name);
            var password = RandomGenerator.GenerateRandomPassword(8);

            while (await _managerService.IsUsernameTaken(username))
            {
                username = RandomGenerator.GenerateRandomString(Store.Name.Length + 4, Store.Name);
            }

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
                Latitude = branchDto.Latitude,
                Longitude = branchDto.Longitude,
                Address = branchDto.BranchAddress,
                Phone = branchDto.BranchPhoneNumber,
                StoreId = branchDto.StoreId,
                BranchManagerId = createdManager.Id,
            };

            var createdBranch = await _branchRepo.CreateBranch(branch);

            BranchEvents.OnBranchCreated(createdBranch);

            return new BranchApprovalDto
            {
                BranchId = createdBranch.Id,
                BranchAddress = createdBranch.Address,
                StoreId = createdBranch.StoreId,
                BranchPhoneNumber = createdBranch.Phone,
                BranchManagerId = createdBranch.BranchManagerId,
                BranchManagerUsername = username,
                BranchManagerPassword = password,
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BranchService]: Error in CreateBranch: {ex.Message}");
            throw new Exception($"An error occurred while creating the branch: {ex.Message}");
        }
    }

    public async Task<BranchInfoDto> GetBranchInfo(string branchId)
    {
        var branch = await _branchRepo.GetBranchById(branchId);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with id {branchId} not found.");
        }

        var ordersInQueue = await _branchRepo.GetOrdersInQueue(branchId);

        var isOpen = TimeService.CheckWithinTimeInterval(branch.StartWorkingHour, branch.EndWorkingHour, branch.WorkingDays);

        return new BranchInfoDto
        {
            BranchId = branch.Id,
            BranchAddress = branch.Address,
            BranchPhoneNumber = branch.Phone,
            StoreId = branch.StoreId,
            StoreName = branch.Store?.Name ?? "Unknown",
            OrdersInQueue = ordersInQueue,
            StartWorkingHour = branch.StartWorkingHour,
            EndWorkingHour = branch.EndWorkingHour,
            IsOpen = isOpen,
            WorkingDays = branch.WorkingDays
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

    public async Task<BranchDto> UpdateWorkingHours(string branchId, BranchWorkingHoursDto workingHoursDto)
    {
        var branch = await _branchRepo.GetBranchById(branchId);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with id {branchId} not found.");
        }
        branch.StartWorkingHour = workingHoursDto.StartWorkingHour;
        branch.EndWorkingHour = workingHoursDto.EndWorkingHour;
        branch.WorkingDays = workingHoursDto.WorkingDays ?? new bool[7];
        await _branchRepo.UpdateBranch(branch);

        return new BranchDto
        {
            BranchId = branch.Id,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            BranchAddress = branch.Address,
            BranchPhoneNumber = branch.Phone,
            StoreId = branch.StoreId,
            StoreName = branch.Store?.Name ?? "Unknown",
            StartWorkingHour = branch.StartWorkingHour,
            EndWorkingHour = branch.EndWorkingHour,
            WorkingDays = branch.WorkingDays,
        };
    }

}