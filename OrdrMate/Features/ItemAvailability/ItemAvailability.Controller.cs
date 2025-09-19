using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdrMate.Features.ItemAvailability;

[ApiController]
[Route("api/[controller]")]
public class ItemAvailabilityController : ControllerBase
{
    private readonly ItemAvailabilityService _itemAvailabilityService;
    private readonly IAuthorizationService _authorizationService;

    public ItemAvailabilityController(
        ItemAvailabilityService itemAvailabilityService,
        IAuthorizationService authorizationService
        )
    {
        _itemAvailabilityService = itemAvailabilityService;
        _authorizationService = authorizationService;
    }

    [HttpPut("toggle")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> ToggleItemAvailability([FromBody] ToggleItemAvailabilityDto dto)
    {
        try
        {
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, dto.BranchId, "BranchManager");
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            var isAvailable = await _itemAvailabilityService.ToggleItemAvailability(dto.ItemId, dto.BranchId);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}