using Microsoft.AspNetCore.Mvc;
using OrdrMate.Managers;

namespace OrdrMate.Features.Orders.Update;

[ApiController]
[Route("api/[controller]")]
public class UpdateOrderController : ControllerBase
{

    private readonly OrderManager _orderManager;

    public UpdateOrderController(OrderManager orderManager)
    {
        _orderManager = orderManager;
    }

    [HttpPut("set-ready/{orderId}")]
    public async Task<IActionResult> SetOrderReady(string orderId)
    {
        try
        {
            await _orderManager.SetOrderReady(orderId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }

}