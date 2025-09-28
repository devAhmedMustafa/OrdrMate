using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.MoveTableReservation;

[ApiController]
[Route("api/Table")]
public class MoveTableReservationController : ControllerBase
{
    private readonly MoveTableReservationService _moveTableReservationService;
    private readonly IAuthorizationService _authorizationService;

    public MoveTableReservationController(
        MoveTableReservationService moveTableReservationService,
        IAuthorizationService authorizationService)
    {
        _moveTableReservationService = moveTableReservationService;
        _authorizationService = authorizationService;
    }

    [HttpPut("move-reservation")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> MoveReservationToAnotherTable([FromBody] MoveOrderDto dto)
    {
        try
        {
            await _moveTableReservationService.MoveOrderToAnotherTable(
                toTableNumber: dto.ToTableNumber,
                reservationId: dto.ReservationId
            );
            return Ok("Reservation moved to new table.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }
}