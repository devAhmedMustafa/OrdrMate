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

    [HttpPut("update-quantity")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> UpdateItemQuantity([FromBody] UpdateItemQuantityDto dto)
    {
        try
        {
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, dto.BranchId, "BranchManager");
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            await _itemAvailabilityService.UpdateItemQuantity(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}