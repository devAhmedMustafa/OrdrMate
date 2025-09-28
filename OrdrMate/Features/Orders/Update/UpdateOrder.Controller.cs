using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Managers;

namespace OrdrMate.Features.Orders.Update;

[ApiController]
[Route("api/[controller]")]
public class UpdateOrderController : ControllerBase
{

    private readonly OrderManager _orderManager;
    private readonly UpdateOrderService _updateOrderService;

    public UpdateOrderController(OrderManager orderManager, UpdateOrderService updateOrderService)
    {
        _orderManager = orderManager;
        _updateOrderService = updateOrderService;
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

    [HttpPut("payment-provider/{orderId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> UpdateOrderPayment(string orderId, [FromBody] PaymentUpdateDto paymentUpdateDto)
    {
        try
        {
            await _updateOrderService.UpdateOrderPayment(orderId, paymentUpdateDto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    [HttpPut("payment-provider/reservation/{reservationId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> UpdateReservationPayment(string reservationId, [FromBody] PaymentUpdateDto paymentUpdateDto)
    {
        try
        {
            await _updateOrderService.UpdateReservationPayment(reservationId, paymentUpdateDto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }

}