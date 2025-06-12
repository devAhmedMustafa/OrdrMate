using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Order;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymobWebhookController(OrderService orderService) : ControllerBase
{
    private readonly OrderService _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] TransactionDto webhookData)
    {
        try
        {
            if (webhookData == null || webhookData.Obj == null)
            {
                return BadRequest("Invalid webhook data.");
            }

            var transaction = webhookData.Obj;

            var order = await _orderService.ProceedTransaction(transaction.Order.Id.ToString(), transaction.Id.ToString());
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while processing the webhook." + ex.Message);
        }
    }
}