using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Order;
using OrdrMate.Managers;
using OrdrMate.Services;
using OrdrMate.Utils;

namespace OrdrMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{

    private readonly OrderService _orderService;
    private readonly BranchService _branchService;
    private readonly OrderManager _orderManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly GeoMaps _geoMaps;

    public OrderController(
        OrderService orderService,
        BranchService branchService,
        IAuthorizationService authorizationService,
        OrderManager orderManager,
        GeoMaps geoMaps
        )
    {
        _orderService = orderService;
        _branchService = branchService;
        _authorizationService = authorizationService;
        _orderManager = orderManager;
        _geoMaps = geoMaps;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderIntentDto>> PlaceOrder([FromBody] PlaceOrderDto placeOrderDto)
    {
        try
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Forbid("User ID not found in claims.");


            placeOrderDto.CustomerId = userId;

            var branch = await _branchService.GetBranchById(placeOrderDto.BranchId);

            if (!TimeService.CheckWithinTimeInterval(branch.StartWorkingHour, branch.EndWorkingHour, branch.WorkingDays))
            {
                return Forbid("Branch is not open at this time.");
            }

            double distance = await _geoMaps.CalculateDistance(
                placeOrderDto.Latitude,
                placeOrderDto.Longitude,
                branch.Latitude,
                branch.Longitude
            );

            if (distance > 50)
            {
                return Forbid($"Order cannot be placed. Distance is {distance:F2} km, which exceeds the 50 km limit.");
            }

            var orderIntent = await _orderService.CreateOrderIntent(placeOrderDto);
            if (orderIntent == null)
            {
                Console.WriteLine("Order placement failed. Order is null.");
                return BadRequest("Failed to place order. Please check your order details and try again.");
            }

            return CreatedAtAction(nameof(PlaceOrder), new { id = orderIntent.OrderIntentId }, orderIntent);

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }

    }

    [HttpGet("check-order-placement-validation/{branchId}")]
    public async Task<ActionResult> CheckOrderPlacementValidation(string branchId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Forbid("User ID not found in claims.");

            var query = HttpContext.Request.Query;
            if (!query.TryGetValue("lat", out var latitude) || !query.TryGetValue("lng", out var longitude))
            {
                return BadRequest("Latitude and longitude are required for order placement validation.");
            }

            var branch = await _branchService.GetBranchById(branchId);

            if (!TimeService.CheckWithinTimeInterval(branch.StartWorkingHour, branch.EndWorkingHour, branch.WorkingDays))
            {
                return Forbid("Branch is not open at this time.");
            }

            var latitudeValue = double.TryParse(latitude, out var lat) ? lat : 0;
            var longitudeValue = double.TryParse(longitude, out var lon) ? lon : 0;

            double distance = await _geoMaps.CalculateDistance(
                latitudeValue,
                longitudeValue,
                branch.Latitude,
                branch.Longitude
            );

            if (distance > 50)
            {
                return Forbid($"Order cannot be placed. Distance is {distance:F2} km, which exceeds the 50 km limit.");
            }

            return Ok("Order placement validation successful.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    [HttpPost("check-prepared/{branchId}/{kitchenName}/{kitchenUnitId}")]
    public async Task<ActionResult> CheckPreparedInQueue(string branchId, string kitchenName, int kitchenUnitId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to check prepared items in this branch.");
            }

            var response = _orderManager.CheckPreparedInQueue(branchId, kitchenName, kitchenUnitId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking prepared items: " + ex.Message);
            return StatusCode(500, $"An error occurred while checking prepared items: {ex.Message}");
        }
    }

    [HttpGet("waiting_times/{branchId}")]
    public async Task<ActionResult<OrderWaitingTimesDto>> GetEstimatedTimes(string branchId)
    {
        try
        {
            var waitingTimes = await _orderManager.GetEstimatedTimes(branchId);
            if (waitingTimes == null)
            {
                return NotFound($"No estimated times found for branch {branchId}.");
            }
            return Ok(waitingTimes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while fetching estimated times: {ex.Message}");
        }
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Forbid("User ID not found in claims.");

            var orders = await _orderService.GetCustomerOrders(userId);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving customer orders: {ex.Message}");
        }
    }

    [HttpGet("detailed/{orderId}")]
    [Authorize(Roles = "Customer, BranchManager, CanManagerRestaurant, Admin")]
    public async Task<ActionResult<OrderDto>> GetOrderDetails(string orderId)
    {
        try
        {
            var order = await _orderService.GetOrderDetails(orderId);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Order with ID {orderId} not found: {ex.Message}");
        }
    }

    [HttpGet("branch/{branchId}/estimated_time/{orderId}")]
    public async Task<ActionResult<decimal>> GetEstimatedTimeForOrder(string branchId, string orderId)
    {
        try
        {
            var estimatedTime = await _orderManager.GetEstimatedTimeForOrder(branchId, orderId);

            return Ok(new { EstimatedTime = estimatedTime });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while fetching estimated time for order {orderId}: {ex.Message}");
        }
    }

    [HttpPut("manual_pay/{orderId}")]
    public async Task<ActionResult> ManualPayOrder(string orderId)
    {
        try
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return NotFound($"Order with ID {orderId} not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, order.BranchId, "BranchManager");
            if (!authorizationResult.Succeeded)
                return Forbid("You do not have permission to manually pay for this order.");

            var result = await _orderService.ManualPayOrder(orderId);
            if (!result)
                return BadRequest("Failed to mark order as paid. Please check the order details and try again.");

            return Ok("Order payment marked as paid successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the payment: {ex.Message}");
        }
    }

    [HttpGet("ready/{branchId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetReadyOrders(string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to view ready orders for this branch.");
            }

            var readyOrders = await _orderService.GetReadyOrders(branchId);
            if (readyOrders == null || !readyOrders.Any()) return NotFound($"No ready orders found for branch {branchId}.");
            return Ok(readyOrders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving ready orders: {ex.Message}");
        }
    }

    [HttpGet("unpaid/{branchId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUnpaidOrders(string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to view ready orders for this branch.");
            }

            var readyOrders = await _orderService.GetUnpaidOrders(branchId);
            if (readyOrders == null || !readyOrders.Any()) return NotFound($"No unpaid orders found for branch {branchId}.");
            return Ok(readyOrders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving ready orders: {ex.Message}");
        }
    }

    [HttpGet("takeaways/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetTakeawayOrders(string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to view takeaway orders for this branch.");
            }

            var takeawayOrders = await _orderService.GetTakeawayOrders(branchId);
            if (takeawayOrders == null || !takeawayOrders.Any())
            {
                return NotFound($"No takeaway orders found for branch {branchId}.");
            }

            return Ok(takeawayOrders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving takeaway orders: {ex.Message}");
        }
    }

    [HttpGet("list/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByBranch(string branchId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByBranch(branchId);
            if (orders == null || !orders.Any())
            {
                return NotFound($"No orders found for branch {branchId}.");
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving orders for branch {branchId}: {ex.Message}");
        }
    }

    [HttpGet("item_queues/{branchId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult> GetItemQueues(string branchId)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to view item queues for this branch.");
            }

            var itemQueues = _orderManager.GetItemQueues(branchId);
            if (itemQueues == null || itemQueues.Count == 0)
            {
                return NotFound($"No item queues found for branch {branchId}.");
            }

            return Ok(itemQueues);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving item queues: {ex.Message}");
        }
    }

    [HttpPost("deliver_request")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult> CreateDeliverRequest([FromBody] DeliverRequestDto deliverRequest)
    {
        try
        {
            if (deliverRequest == null || string.IsNullOrEmpty(deliverRequest.OrderId))
            {
                return BadRequest("Order ID is required to create a deliver request.");
            }

            var order = await _orderService.GetOrderById(deliverRequest.OrderId);
            if (order == null)
            {
                return NotFound($"Order with ID {deliverRequest.OrderId} not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, order.BranchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to create a deliver request for this order.");
            }

            var result = await _orderService.CreateDeliverRequest(deliverRequest.OrderId);
            if (result == null)
            {
                return NotFound($"Deliver request for order {deliverRequest.OrderId} not found.");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating deliver request for order {deliverRequest.OrderId}: {ex.Message}");
            return StatusCode(500, $"An error occurred while creating the deliver request: {ex.Message}");
        }
    }

    [HttpPost("confirm_deliver/{orderId}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<DeliverRequestDto>> ConfirmDelivery(string orderId)
    {
        try
        {
            var result = await _orderService.ConfirmDeliverRequest(orderId);
            if (!result)
            {
                return NotFound($"Deliver request for order {orderId} not found.");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming deliver request for order {orderId}: {ex.Message}");
            return StatusCode(500, $"An error occurred while confirming the deliver request: {ex.Message}");
        }
    }

    [HttpDelete("cancel_deliver/{orderId}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> CancelDelivery(string orderId)
    {
        try
        {
            var result = await _orderService.CancelDeliverRequest(orderId);
            if (!result)
            {
                return NotFound($"Deliver request for order {orderId} not found.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while canceling the deliver request: {ex.Message}");
        }
    }

    [HttpPut("pick/{orderId}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderInvoiceDto>> PickOrder(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Forbid("User ID not found in claims.");

            var order = await _orderService.PickOrder(orderId);

            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found or cannot be picked.");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while picking the order: {ex.Message}");
        }
    }

    [HttpPut("mark_paid/{orderId}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<ActionResult> MarkOrderAsPaid(string orderId)
    {
        try
        {
            var order = await _orderService.GetOrderById(orderId);

            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, order.BranchId, "BranchManager");
            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to mark this order as paid.");
            }

            var result = await _orderService.MarkOrderAsPaid(orderId);
            if (!result)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while marking the order as paid: {ex.Message}");
        }
    }
    [HttpPut("cancel/{orderId}")]
public async Task<IActionResult> CancelOrder(string orderId)
{
    var result = await _orderService.CancelOrderAsync(orderId);
    if (!result)
        return NotFound(new { message = "Order not found or already cancelled." });

    return Ok(new { message = "Order cancelled successfully." });
}

}