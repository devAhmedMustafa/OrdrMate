using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Order;
using OrdrMate.Utils.Exceptions;

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
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
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
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving shifts: {ex.Message}");
        }
    }

    [HttpPost("new/start")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<BranchShift>> StartShift([FromBody] StartShiftDto startShiftDto)
    {
        try
        {
            var shift = await _branchShiftService.StartShiftAsync(startShiftDto.BranchId);
            return Ok(shift);
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
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
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while ending the shift: {ex.Message}");
        }
    }
    [HttpGet("{shiftId}/orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersForShift(string shiftId)
    {
        try
        {
            var orders = await _branchShiftService.GetOrdersForShift(shiftId);
            return Ok(orders);
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving orders for the shift: {ex.Message}");
        }
    }

    [HttpGet("{shiftId}/orders/csv")]
    public async Task<ActionResult> GetOrdersForShiftCsv(string shiftId)
    {
        try
        {
            var csv = await _branchShiftService.GetOrdersForShiftCsv(shiftId);
            return File(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "text/csv", $"orders_{shiftId}.csv");
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving orders for the shift: {ex.Message}");
        }
    }
}