using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OrdrMate.DTOs.Order;
using OrdrMate.Managers;
using OrdrMate.Services;
using OrdrMate.Sockets;

namespace OrdrMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{

    private readonly OrderService _orderService;
    private readonly OrderManager _orderManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly CustomerOrdersSocketHandler _customerOrderSocketHandler;

    public OrderController(
        OrderService orderService,
        IAuthorizationService authorizationService,
        OrderManager orderManager,
        CustomerOrdersSocketHandler customerOrderSocketHandler)
    {
        _orderService = orderService;
        _authorizationService = authorizationService;
        _orderManager = orderManager;
        _customerOrderSocketHandler = customerOrderSocketHandler;
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

            _orderManager.CheckPreparedInQueue(branchId, kitchenName, kitchenUnitId);
            return Ok("Item checked and next item sent to clients.");
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

}