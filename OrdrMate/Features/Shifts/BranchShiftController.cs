using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("start")]
        [Authorize(Roles = "BranchManager")]
        public async Task<ActionResult<BranchShift>> StartShift([FromBody] StartShiftDto startShiftDto)
        {
            try
            {
                var shift = await _branchShiftService.StartShiftAsync(startShiftDto.BranchId, startShiftDto.StartTime);
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
                var shift = await _branchShiftService.EndShiftAsync(endShiftDto.BranchId, endShiftDto.EndTime);
                if (shift == null)
                    return NotFound("No ongoing shift found for the branch.");

                return Ok(shift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while ending the shift: {ex.Message}");
            }
        }
    }
