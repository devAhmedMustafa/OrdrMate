using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Branch;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Sockets;
using OrdrMate.Utils;

namespace OrdrMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchRequestRepo _branchRequestRepo;
    private readonly IAuthorizationService _authorizationService;
    private readonly BranchService _branchService;
    private readonly BranchSocketHandler _branchSocketHandler;

    public BranchController(
        IBranchRequestRepo branchRequestRepo,
        IAuthorizationService authorizationService,
        BranchService branchService,
        BranchSocketHandler branchSocketHandler
    )
    {
        _authorizationService = authorizationService;
        _branchRequestRepo = branchRequestRepo;
        _branchService = branchService;
        _branchSocketHandler = branchSocketHandler;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAllBranchRequests()
    {
        var branchRequests = await _branchRequestRepo.GetAllBranchRequests();

        var branchRequestsDto = branchRequests.Select(br => new BranchRequestDto
        {
            BranchRequestId = br.Id,
            PharmacyName = br.Pharmacy.Name,
            BranchAddress = br.Address,
            BranchPhoneNumber = br.Phone,
            Lantitude = br.Latitude,
            Longitude = br.Longitude

        }).ToList();
        return Ok(branchRequestsDto);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetBranchRequestById(string id)
    {
        var branchRequest = await _branchRequestRepo.GetBranchRequestById(id);
        if (branchRequest == null)
        {
            return NotFound($"Branch request with id {id} not found.");
        }
        var branchRequestDto = new BranchRequestDto
        {
            BranchRequestId = branchRequest.Id,
            PharmacyName = branchRequest.Pharmacy.Name,
            BranchAddress = branchRequest.Address,
            BranchPhoneNumber = branchRequest.Phone,
            Lantitude = branchRequest.Latitude,
            Longitude = branchRequest.Longitude
        };
        return Ok(branchRequestDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBranchRequest([FromBody] AddBranchRequestDto branchRequestDto)
    {

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchRequestDto.PharmacyId, "CanManageRestaurant");

        if (!authorizationResult.Succeeded)
        {
            return Forbid("You do not have permission to manage this restaurant.");
        }

        if (branchRequestDto == null)
        {
            return BadRequest("Branch request data is required.");
        }

        var branchRequest = new Models.BranchRequest
        {
            Id = Guid.NewGuid().ToString(),
            PharmacyId = branchRequestDto.PharmacyId,
            Address = branchRequestDto.BranchAddress,
            Phone = branchRequestDto.BranchPhoneNumber,
            Latitude = branchRequestDto.Lantitude,
            Longitude = branchRequestDto.Longitude
        };

        var createdBranchRequest = await _branchRequestRepo.CreateBranchRequest(branchRequest);
        return CreatedAtAction(nameof(GetBranchRequestById), new { id = createdBranchRequest.Id }, new BranchRequestDto
        {
            BranchRequestId = createdBranchRequest.Id,
            PharmacyName = createdBranchRequest.Pharmacy.Name,
            BranchAddress = createdBranchRequest.Address,
            BranchPhoneNumber = createdBranchRequest.Phone,
            Lantitude = createdBranchRequest.Latitude,
            Longitude = createdBranchRequest.Longitude
        });
    }

    [HttpPost("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ApproveBranchRequest(string id)
    {
        var branchRequest = await _branchRequestRepo.GetBranchRequestById(id);
        if (branchRequest == null)
        {
            return NotFound($"Branch request with id {id} not found.");
        }

        var branchCreated = await _branchService.CreateBranch(new BranchDto
        {
            Latitude = branchRequest.Latitude,
            Longitude = branchRequest.Longitude,
            BranchAddress = branchRequest.Address,
            BranchPhoneNumber = branchRequest.Phone,
            RestaurantId = branchRequest.PharmacyId
        });

        if (branchCreated == null)
        {
            return BadRequest("Failed to create branch.");
        }

        var isDeleted = await _branchRequestRepo.DeleteBranchRequest(id);
        if (!isDeleted)
        {
            return BadRequest("Failed to delete branch request.");
        }

        return CreatedAtAction(nameof(GetBranchRequestById), new { id = branchCreated.BranchId }, branchCreated);
    }

    [HttpGet]
    [Route("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetRestaurantBranches(string restaurantId)
    {
        try
        {
            var branches = await _branchService.GetRestaurantBranches(restaurantId);
            return Ok(branches);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Restaurant with ID {restaurantId} not found: {ex.Message}");
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetAllBranches()
    {
        try
        {
            var branches = await _branchService.GetAllBranches();
            return Ok(branches);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving branches: {ex.Message}");
        }
    }

    [HttpGet("info/{branchId}")]
    public async Task<ActionResult<BranchInfoDto>> GetBranchInfo(string branchId)
    {
        try
        {
            var branchInfo = await _branchService.GetBranchInfo(branchId);
            if (branchInfo == null)
            {
                return NotFound($"Branch with ID {branchId} not found.");
            }

            return Ok(branchInfo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
    }

    [HttpGet("live/{branchId}")]
    public async Task Socket(string branchId)
    {
        if (_branchSocketHandler == null)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsync("BranchOrdersSocketHandler is not initialized.");
            return;
        }
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsync("WebSocket request expected.");
            return;
        }

        var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"WebSocket connection established for branch {branchId}.");
        await _branchSocketHandler.AddSocketAsync(branchId, socket);
    }

    [HttpGet("balance/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchBalanceDto>> GetBranchBalance(string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");

            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to access this branch balance.");
            }

            var balance = await _branchService.GetBranchBalance(branchId);
            if (balance == null)
            {
                return NotFound($"Branch with ID {branchId} not found.");
            }

            return Ok(balance);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving branch balance: {ex.Message}");
        }
    }

    [HttpGet("is-open/{branchId}")]
    public async Task<ActionResult<bool>> IsBranchOpen(string branchId)
    {
        try
        {
            var branch = await _branchService.GetBranchById(branchId);
            if (branch == null)
            {
                return NotFound($"Branch with ID {branchId} not found.");
            }
            var isOpen = TimeService.CheckWithinTimeInterval(branch.StartWorkingHour, branch.EndWorkingHour, branch.WorkingDays);
            return Ok(isOpen);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
    }

    [HttpGet("working-hours/{branchId}")]
    public async Task<ActionResult<BranchWorkingHoursDto>> GetBranchWorkingHours(string branchId)
    {
        try
        {
            var branch = await _branchService.GetBranchById(branchId);
            if (branch == null)
            {
                return NotFound($"Branch with ID {branchId} not found.");
            }
            return Ok(new BranchWorkingHoursDto
            {
                StartWorkingHour = branch.StartWorkingHour,
                EndWorkingHour = branch.EndWorkingHour,
                WorkingDays = branch.WorkingDays
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
    }

    [HttpPut("working-hours/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchDto>> UpdateWorkingHours(string branchId, [FromBody] BranchWorkingHoursDto workingHoursDto)
    {
        if (workingHoursDto == null)
        {
            return BadRequest("Working hours data is required.");
        }
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to update working hours for this branch.");
            }
            Console.WriteLine($"Updating working hours for branch {branchId} with data: {workingHoursDto.StartWorkingHour}, {workingHoursDto.EndWorkingHour}, {string.Join(", ", workingHoursDto.WorkingDays ?? new bool[7])}");
            var updatedBranch = await _branchService.UpdateWorkingHours(branchId, workingHoursDto);
            return Ok(updatedBranch);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating working hours: {ex.Message}");
        }
    }

    [HttpGet("get-by-id/{branchId}")]
    public async Task<ActionResult<BranchDto>> GetBranchById(string branchId)
    {
        try
        {
            var branch = await _branchService.GetBranchById(branchId);
            if (branch == null)
            {
                return NotFound($"Branch with ID {branchId} not found.");
            }
            return Ok(branch);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
    }
}