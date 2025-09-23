using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.ShareReservation;

[ApiController]
[Route("api/[controller]")]
public class ShareReservationController : ControllerBase
{
    private readonly ShareReservationService _shareReservationService;

    public ShareReservationController(ShareReservationService shareReservationService)
    {
        _shareReservationService = shareReservationService;
    }

    [HttpPost("generate-link")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GenerateShareableLink([FromBody] GenerateLinkRequest request)
    {
        try
        {
            var link = await _shareReservationService.GenerateShareableLinkAsync(request.ReservationId);
            return Ok(new { Link = link });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}