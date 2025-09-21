using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Features.Customization.DTOs;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Customization;

[ApiController]
[Route("api/[controller]")]
public class UserCustomizationController : ControllerBase
{
    private readonly UserCustomizationService _service;
    private readonly IAuthorizationService _authorizationService;

    public UserCustomizationController(
        UserCustomizationService service,
        IAuthorizationService authorizationService)
    {
        _service = service;
        _authorizationService = authorizationService;
    }

    [HttpGet("{orderId}")]
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
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InternalServerException ex)
        {
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }
    
}