using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.DistributionSync;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{

    private readonly SyncService _syncService;
    
    public SyncController(SyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("push/order")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> PushOrder([FromBody] PushOrderRequest req)
    {
        try
        {
            await _syncService.PushOrder(req);
            return Ok(new { message = "Order pushed successfully." });
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(new { message = nfEx.Message });
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(new { message = brEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.Message });
        }
    }
}