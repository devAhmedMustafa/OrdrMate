namespace OrdrMate.Features.TableReservation.DistributionSync;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/TableReservation/[controller]")]
public class DistributionSyncController : ControllerBase
{
    private readonly DistributionSyncService _distributionSyncService;

    public DistributionSyncController(DistributionSyncService distributionSyncService)
    {
        _distributionSyncService = distributionSyncService;
    }

    [HttpPost("sync/update")]
    public async Task<IActionResult> UpdateSync([FromBody] UpdateSyncRequest request)
    {
        try
        {
            var result = await _distributionSyncService.UpdateDataSync(request);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
