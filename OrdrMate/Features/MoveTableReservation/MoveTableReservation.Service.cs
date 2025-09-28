using Microsoft.AspNetCore.Http.HttpResults;
using OrdrMate.Managers;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.MoveTableReservation;

public class MoveTableReservationService
{
    
    private readonly TableManager _tableManager;
    private readonly ITableRepo _tableRepo;

    public MoveTableReservationService(TableManager tableManager, ITableRepo tableRepo)
    {
        _tableManager = tableManager;
        _tableRepo = tableRepo;

    }

    public async Task MoveOrderToAnotherTable(int toTableNumber, string reservationId)
    {
        try
        {
            var reservation = await _tableRepo.GetTableReservationById(reservationId);
            if (reservation == null)
                throw new NotFoundException("Reservation not found.");

            var targetTable = await _tableRepo.GetTableByNumber(reservation.BranchId, toTableNumber);
            if (targetTable == null)
                throw new NotFoundException("Target table not found.");

            await _tableManager.MoveTableReservation(
                reservation.BranchId,
                reservation.TableNumber,
                targetTable.TableNumber,
                reservation.ReservationId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error moving reservation: {ex.Message}", ex);
        }
    }
}