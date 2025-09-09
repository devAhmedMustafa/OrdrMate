using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.BranchAttendance;

[ApiController]
[Route("api/branch-attendance")]
public class BranchAttendanceController : ControllerBase
{
    private readonly BranchAttendanceService _branchAttendanceService;

    public BranchAttendanceController(BranchAttendanceService branchAttendanceService)
    {
        _branchAttendanceService = branchAttendanceService;
    }

    [HttpPut("confirm-table")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ConfirmTableReservation([FromBody] ConfirmTableRequest request)
    {
        try
        {
            await _branchAttendanceService.ConfirmTableReservation(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}