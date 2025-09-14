namespace OrdrMate.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Customization;
using OrdrMate.Features.Customization.DTOs;
using OrdrMate.Services;

[ApiController]
[Route("api/[controller]")]
public class CustomizationController : ControllerBase
{
    private readonly CustomizationService _customizationService;
    private readonly IAuthorizationService _authorizationService;

    public CustomizationController(
        CustomizationService customizationService,
        IAuthorizationService authorizationService
        )
    {
        _customizationService = customizationService;
        _authorizationService = authorizationService;
    }

    [HttpGet("restaurant/categories/{restaurantId}")]
    public async Task<IActionResult> GetCustomizationCategories(string restaurantId)
    {
        if (string.IsNullOrEmpty(restaurantId))
        {
            return BadRequest("Restaurant ID is required.");
        }

        var categories = await _customizationService.GetCustomizationCategories(restaurantId);
        foreach (var category in categories)
        {
            Console.WriteLine($"Metadata for category {category.Metadata}");
        }
        return Ok(categories);
    }

    [HttpPost("create-category-single-select")]
    public async Task<IActionResult> CreateCustomizationCategory([FromBody] CreateSingleSelectDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest("Category data is required.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, categoryDto.RestaurantId, "CanManageRestaurant");
        if (!authorizationResult.Succeeded)
        {
            return Forbid("You do not have permission to create customization categories.");
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
    public async Task<IActionResult> AssignCategoryToItem([FromBody] AssignCustomizationCategoryDto assignDto)
    {
        if (assignDto == null || string.IsNullOrEmpty(assignDto.ItemId) || string.IsNullOrEmpty(assignDto.CategoryId))
        {
            return BadRequest("Item ID and Category ID are required.");
        }

        try
        {
            await _customizationService.AssignCategoryToItem(assignDto.ItemId, assignDto.CategoryId);
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

    [HttpGet("item/{itemId}/customizations")]
    public async Task<IActionResult> GetItemCustomizations(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return BadRequest("Item ID is required.");
        }

        var customizations = await _customizationService.GetItemCustomizations(itemId);
        return Ok(customizations);
    }

}