using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OrdrMate.DTOs.Order;
using OrdrMate.Services;

namespace OrdrMate.Features.Orders.ShareReservation;

[ApiController]
[Route("api/[controller]")]
public class ShareReservationController : ControllerBase
{
    private readonly ShareReservationService _shareReservationService;
    private readonly OrderService _orderService;

    public ShareReservationController(ShareReservationService shareReservationService, OrderService orderService)
    {
        _shareReservationService = shareReservationService;
        _orderService = orderService;
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

    [HttpGet("add-order")]
    [Authorize(AuthenticationSchemes = "JwtBearer,ShareReservationJwt")]
    public async Task<ActionResult<OrderDto>> AddOrder([FromQuery] PlaceOrderDto request)
    {
        try
        {
            var reservationId = User.FindFirst("reservationId")?.Value;
            if (string.IsNullOrEmpty(reservationId))
            {
                return BadRequest(new { Message = "Invalid reservation ID." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid("User ID not found in claims.");
            }

            request.CustomerId = userId;

            var result = await _orderService.CreateOrderIntent(request, reservationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ex.Message });
        }
    }
}