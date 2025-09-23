using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.Orders.ShareReservation;

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
    public IActionResult GenerateShareableLink([FromBody] GenerateLinkRequest request)
    {
        try
        {
            var link = _shareReservationService.GenerateShareableLink(request.ReservationId);
            return Ok(new { Link = link });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ex.Message });
        }
    }
}