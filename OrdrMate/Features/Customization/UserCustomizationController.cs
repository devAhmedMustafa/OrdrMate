using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Features.Customization.DTOs;

namespace OrdrMate.Features.Customization;

[ApiController]
[Route("api/[controller]")]
public class UserCustomizationController : ControllerBase
{
    private readonly UserCustomizationService _service;

    public UserCustomizationController(UserCustomizationService service)
    {
        _service = service;
    }

    [HttpGet("{orderId}")]
    [Authorize(Roles = "Customer, BranchManager")]
    public async Task<ActionResult<OrderItemsCustomizationResponseDto>> GetOrderCustomizations(string orderId)
    {
        try
        {
            var result = await _service.GetOrderCustomizationsAsync(orderId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }
    
}