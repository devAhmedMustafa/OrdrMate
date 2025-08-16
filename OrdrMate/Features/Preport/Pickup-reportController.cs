using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs;
using OrdrMate.Services;
namespace OrdrMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PickupReportController : ControllerBase
    {
        private readonly PickupReportService _pickupReportService;

        public PickupReportController(PickupReportService pickupReportService)
        {
            _pickupReportService = pickupReportService;
        }

        [HttpPost("reportPickup")]
        public async Task<IActionResult> ReportPickup([FromBody] PickupReportDto report)
        {
            var result = await _pickupReportService.ReportPickupAsync(report);
            if (result)
                return Ok(new { message = "Pickup report created successfully." });
            else
                return BadRequest(new { message = "Unable to create pickup report. Ensure it is within 30 minutes of readiness." });
        }

        [HttpPost("cancelReport/{reportId}")]
        public async Task<IActionResult> CancelReport(string reportId)
        {
            var result = await _pickupReportService.CancelReportAsync(reportId);
            if (result)
                return Ok(new { message = "Report canceled successfully." });
            else
                return BadRequest(new { message = "Unable to cancel report. Ensure it is within 20 minutes of being reported." });
        }

        [HttpPost("approvePickup/{reportId}")]
        public async Task<IActionResult> ApprovePickup(string reportId)
        {
            var result = await _pickupReportService.ApprovePickupAsync(reportId);
            if (result)
                return Ok(new { message = "Pickup approved successfully." });
            else
                return BadRequest(new { message = "Unable to approve pickup. Ensure the report is within 30 minutes of reporting." });
        }
    }
}
