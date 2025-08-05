namespace OrdrMate.Controllers;

using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Customization;
using OrdrMate.Services;

[ApiController]
[Route("api/[controller]")]
public class CustomizationController : ControllerBase
{
    private readonly CustomizationService _customizationService;

    public CustomizationController(CustomizationService customizationService)
    {
        _customizationService = customizationService;
    }

    [HttpPost("create-category-single-select")]
    public async Task<IActionResult> CreateCustomizationCategory([FromBody] CreateSingleSelectDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest("Category data is required.");
        }
        try
        {
            await _customizationService.CreateCustomizationCategory(categoryDto);
            return Ok("Customization category created successfully.");
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("assign-category-to-item")]
    public async Task<IActionResult> AssignCategoryToItem([FromQuery] string itemId, [FromQuery] string categoryId)
    {
        if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(categoryId))
        {
            return BadRequest("Item ID and Category ID are required.");
        }

        try
        {
            await _customizationService.AssignCategoryToItem(itemId, categoryId);
            return Ok("Category assigned to item successfully.");
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}