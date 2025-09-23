using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    [HttpGet("access-reservation")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme+",ShareReservationJwt")]
    public IActionResult AccessSharedReservation()
    {
        try
        {
            var token = HttpContext.Request.Headers["x-share-reservation-token"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Message = "Token is required." });
            }

            var reservationId = User.FindFirst("reservationId")?.Value;
            if (string.IsNullOrEmpty(reservationId))
            {
                return Unauthorized(new { Message = "Invalid or expired token." });
            }

            _shareReservationService.AccessSharedReservation(token);
            return Ok(new { ReservationId = reservationId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ex.Message });
        }
    }
}