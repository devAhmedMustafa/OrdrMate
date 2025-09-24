using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.FreezeTable;

[ApiController]
[Route("api/Table")]
public class FreezeTableController : ControllerBase
{

    private readonly FreezeTableService _tableService;
    private readonly IAuthorizationService _authorizationService;
    public FreezeTableController(
        FreezeTableService tableService,
        IAuthorizationService authorizationService)
    {
        _tableService = tableService;
        _authorizationService = authorizationService;
    }

    [HttpPut("freeze/{branchId}/{tableNumber}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> FreezeTable(string branchId, int tableNumber)
    {
        try
        {
            var authorization = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorization.Succeeded)
            {
                return Forbid("You are not authorized to freeze this table.");
            }
            var result = await _tableService.FreezeTable(branchId, tableNumber);
            if (!result) return NotFound("Table not found.");
            return Ok("Table frozen.");
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (UnauthorizedException unEx)
        {
            return Unauthorized(unEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("unfreeze/{branchId}/{tableNumber}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> UnfreezeTable(string branchId, int tableNumber)
    {
        try
        {
            var authorization = await _authorizationService.AuthorizeAsync(User, branchId, "BranchManager");
            if (!authorization.Succeeded)
            {
                return Forbid();
            }

            var result = await _tableService.UnfreezeTable(branchId, tableNumber);
            if (!result) return NotFound("Table not found.");
            return Ok("Table unfrozen.");
        }
        catch (NotFoundException nfEx)
        {
            return NotFound(nfEx.Message);
        }
        catch (BadRequestException brEx)
        {
            return BadRequest(brEx.Message);
        }
        catch (UnauthorizedException unEx)
        {
            return Unauthorized(unEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    

}