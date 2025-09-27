namespace OrdrMate.Features.Orders.TableReservation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Order;

[ApiController]
[Route("api/[controller]")]
public class TableReservationController : ControllerBase
{
    
    private readonly TableReservationService _tableReservationService;

    public TableReservationController(TableReservationService tableReservationService)
    {
        _tableReservationService = tableReservationService;
    }

    [HttpPut("push-to-kitchen/{reservationId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<OrderDto>> PushToKitchen(string reservationId)
    {
        try
        {
            var response = await _tableReservationService.PushToKitchenAsync(reservationId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}