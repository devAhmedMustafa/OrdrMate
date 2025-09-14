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
    PharmacyService PharmacyService,
    OrderManager orderManager,
    OrderService orderService
    )
{
    private readonly IBranchRepo _branchRepo = branchRepo;
    private readonly ManagerService _managerService = managerService;
    private readonly OrderManager _orderManager = orderManager;
    private readonly PharmacyService _PharmacyService = PharmacyService;
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
            PharmacyId = branch.PharmacyId,
            PharmacyName = branch.Pharmacy?.Name ?? "Unknown",
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
            PharmacyId = b.PharmacyId,
            PharmacyName = b.Pharmacy?.Name ?? "Unknown",
            IsOpen = TimeService.CheckWithinTimeInterval(b.StartWorkingHour, b.EndWorkingHour, b.WorkingDays),
            BranchPhoneNumber = b.Phone,
            LogoUrl = b.Pharmacy?.Profile?.LogoUrl,
        });
    }

    public async Task<IEnumerable<BranchDto>> GetPharmacyBranches(string PharmacyId)
    {
        var branches = await _branchRepo.GetPharmacyBranches(PharmacyId);
        return branches.Select(b => new BranchDto
        {
            BranchId = b.Id,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            BranchAddress = b.Address,
            BranchPhoneNumber = b.Phone,
            PharmacyId = b.PharmacyId,
            PharmacyName = b.Pharmacy?.Name ?? "Unknown",
        });
    }

    public async Task<BranchApprovalDto> CreateBranch(CreateBranchDto branchDto)
    {

        var Pharmacy = await _PharmacyService.GetPharmacyById(branchDto.PharmacyId);

        if (Pharmacy == null) throw new KeyNotFoundException($"Pharmacy with id {branchDto.PharmacyId} not found.");
        

        var username = RandomGenerator.GenerateRandomString(Pharmacy.Name.Length + 4, Pharmacy.Name);
        var password = RandomGenerator.GenerateRandomPassword(8);

        while (await _managerService.IsUsernameTaken(username))
        {
            username = RandomGenerator.GenerateRandomString(Pharmacy.Name.Length + 4, Pharmacy.Name);
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
            PharmacyId = branchDto.PharmacyId,
            BranchManagerId = createdManager.Id,
        };

        var createdBranch = await _branchRepo.CreateBranch(branch);

        BranchEvents.OnBranchCreated(createdBranch);

        return new BranchApprovalDto
        {
            BranchId = createdBranch.Id,
            BranchAddress = createdBranch.Address,
            PharmacyId = createdBranch.PharmacyId,
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
        var waitingTimes = await _orderManager.GetEstimatedTimes(branchId);

        var isOpen = TimeService.CheckWithinTimeInterval(branch.StartWorkingHour, branch.EndWorkingHour, branch.WorkingDays);

        return new BranchInfoDto
        {
            BranchId = branch.Id,
            BranchAddress = branch.Address,
            BranchPhoneNumber = branch.Phone,
            PharmacyId = branch.PharmacyId,
            PharmacyName = branch.Pharmacy?.Name ?? "Unknown",
            OrdersInQueue = ordersInQueue,
            MinWaitingTime = waitingTimes.MinWaitingTime,
            MaxWaitingTime = waitingTimes.MaxWaitingTime,
            AverageWaitingTime = waitingTimes.AverageWaitingTime,
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
            PharmacyId = branch.PharmacyId,
            PharmacyName = branch.Pharmacy?.Name ?? "Unknown",
            StartWorkingHour = branch.StartWorkingHour,
            EndWorkingHour = branch.EndWorkingHour
        };
    }

}