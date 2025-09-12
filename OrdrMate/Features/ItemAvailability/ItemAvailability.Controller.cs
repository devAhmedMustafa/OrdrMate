using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.ItemAvailability;

[ApiController]
[Route("api/[controller]")]
public class ItemAvailabilityController : ControllerBase
{
    private readonly ItemAvailabilityService _itemAvailabilityService;

    public ItemAvailabilityController(ItemAvailabilityService itemAvailabilityService)
    {
        _itemAvailabilityService = itemAvailabilityService;
    }

    [HttpPut("toggle")]
    public async Task<IActionResult> ToggleItemAvailability([FromBody] ToggleItemAvailabilityDto dto)
    {
        try
        {
            var isAvailable = await _itemAvailabilityService.ToggleItemAvailability(dto.ItemId, dto.BranchId);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}