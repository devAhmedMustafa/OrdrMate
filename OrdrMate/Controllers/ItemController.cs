using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.DTOs.Item;
using OrdrMate.Services;

namespace OrdrMate.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ItemController : ControllerBase
{
    private readonly ItemService _service;
    private readonly IAuthorizationService _authorizationService;

    public ItemController(ItemService service, IAuthorizationService authorizationService)
    {
        _service = service;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItem([FromBody] AddItemDto dto)
    {
        try
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, dto.RestaurantId, "CanManageRestaurant");

            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to manage this restaurant.");
            }

            var result = await _service.AddItem(dto);
            return CreatedAtAction(nameof(CreateItem), new { id = result?.Id }, result);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("already exists"))
                return Conflict(new { err = e.Message });

            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetItemsByRestaurantId(string restaurantId)
    {
        try
        {
            var items = await _service.GetItemsByRestaurantId(restaurantId);
            return Ok(items);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItem(string id)
    {
        try
        {
            var item = await _service.GetItem(id);
            if (item == null)
            {
                return NotFound(new { err = "Item not found" });
            }
            return Ok(item);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> UpdateItem(string id, [FromBody] UpdateItemDto dto)
    {
        try
        {

            var itemToUpdate = await _service.GetItemAuthInfo(id);

            if (itemToUpdate == null)
            {
                return NotFound(new { err = "Item not found" });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, itemToUpdate.RestaurantId, "CanManageRestaurant");

            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to manage this restaurant.");
            }

            var item = await _service.UpdateItem(id, dto);
            if (item == null)
            {
                return NotFound(new { err = "Item not found" });
            }
            return Ok(item);
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItem(string id)
    {
        try
        {
            var itemToDelete = await _service.GetItemAuthInfo(id);

            if (itemToDelete == null)
            {
                return NotFound(new { err = "Item not found" });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, itemToDelete.RestaurantId, "CanManageRestaurant");

            if (!authorizationResult.Succeeded)
            {
                return Forbid("You do not have permission to manage this restaurant.");
            }

            var result = await _service.DeleteItem(id);
            if (!result)
            {
                return NotFound(new { err = "Item not found" });
            }
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(new { err = e.Message });
        }
    }

    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetBranchItems(string branchId)
    {
        try
        {
            var items = await _service.GetAvailableItems(branchId);
            if (items == null || !items.Any())
            {
                return NotFound($"No items found for branch with ID {branchId}.");
            }
            return Ok(items);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Branch with ID {branchId} not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving branch items: {ex.Message}");
        }
    }

}
