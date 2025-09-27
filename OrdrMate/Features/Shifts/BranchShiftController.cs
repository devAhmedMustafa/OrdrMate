using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Models;

namespace OrdrMate.Features.Shifts;

[ApiController]
[Route("api/[controller]")]
public class BranchShiftController : ControllerBase
{
    private readonly BranchShiftService _branchShiftService;

    public BranchShiftController(BranchShiftService branchShiftService)
    {
        _branchShiftService = branchShiftService;
    }

    [HttpGet("status/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchShift>> GetCurrentShiftStatus(string branchId)
    {
        try
        {
            var shift = await _branchShiftService.GetCurrentShiftStatusAsync(branchId);
            return Ok(shift);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the shift status: {ex.Message}");
        }
    }

    [HttpGet("list/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<IEnumerable<BranchShift>>> GetAllShiftsForBranch(string branchId)
    {
        try
        {
            var shifts = await _branchShiftService.GetAllShiftsForBranchAsync(branchId);
            return Ok(shifts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving shifts: {ex.Message}");
        }
    }

    [HttpPost("start")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchShift>> StartShift([FromBody] StartShiftDto startShiftDto)
    {
        try
        {
            var shift = await _branchShiftService.StartShiftAsync(startShiftDto.BranchId);
            return Ok(shift);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while starting the shift: {ex.Message}");
        }
    }

    [HttpPost("end")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchShift>> EndShift([FromBody] EndShiftDto endShiftDto)
    {
        try
        {
            var shift = await _branchShiftService.EndShiftAsync(endShiftDto.BranchId);
            if (shift == null)
                return NotFound("No ongoing shift found for the branch.");

            return Ok(shift);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while ending the shift: {ex.Message}");
        }
    }
    [HttpGet("{shiftId}/orders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersForShift(string shiftId)
    {
        try
        {
            var orders = await _branchShiftService.GetOrdersForShift(shiftId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving orders for the shift: {ex.Message}");
        }
    }
}