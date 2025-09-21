using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Order;

namespace OrdrMate.Features.BranchAttendance;

[ApiController]
[Route("api/[controller]")]
public class BranchAttendanceController : ControllerBase
{
    private readonly BranchAttendanceService _branchAttendanceService;
    private readonly IAuthorizationService _authorizationService;

    public BranchAttendanceController(
        BranchAttendanceService branchAttendanceService,
        IAuthorizationService authorizationService
        )
    {
        _branchAttendanceService = branchAttendanceService;
        _authorizationService = authorizationService;
    }

    [HttpPut("confirm-table")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderDto>> ConfirmTableReservation([FromBody] ConfirmTableRequest request)
    {
        try
        {
            var order = await _branchAttendanceService.ConfirmTableReservation(request);
            return Ok(order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BranchAttendanceController] Error confirming table reservation: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> GetBranchAttendance([FromQuery] string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");

            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to access this branch balance.");
            }

            var attendance = _branchAttendanceService.GetBranchAttendanceCode(branchId);
            return Ok(new { code = attendance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("confirm-attendance-via-manager/{branchId}/{table}")]
    public async Task<IActionResult> ConfirmBranchAttendance(string branchId, int table)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to confirm attendance for this branch.");
            }
            
            var isValid = await _branchAttendanceService.DirectTableSeating(branchId, table);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}