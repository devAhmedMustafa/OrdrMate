using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.Orders.Tax;

[ApiController]
[Route("api/[controller]")]
public class OrderTaxController : ControllerBase
{
    private readonly OrderTaxService _orderTaxService;
    private readonly IAuthorizationService _authorizationService;

    public OrderTaxController(OrderTaxService orderTaxService, IAuthorizationService authorizationService)
    {
        _orderTaxService = orderTaxService;
        _authorizationService = authorizationService;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrderTax([FromBody] UpdateTaxRequest request)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, request.RestaurantId, "CanManageRestaurant");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await _orderTaxService.UpdateOrderTax(request.RestaurantId, request.NewTax);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the order tax.", details = ex.Message });
        }
    }

    [HttpGet("{restaurantId}")]
    public async Task<IActionResult> GetOrderTax(string restaurantId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, restaurantId, "CanManageRestaurant");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var tax = await _orderTaxService.GetOrderTax(restaurantId);
            return Ok(new { tax });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the order tax.", details = ex.Message });
        }
    }
}