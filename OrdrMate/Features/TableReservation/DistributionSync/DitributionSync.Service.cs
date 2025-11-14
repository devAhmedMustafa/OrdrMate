using OrdrMate.Data;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.TableReservation.DistributionSync;

public class DistributionSyncService
{
    
    private readonly OrdrMateDbContext _db;

    public DistributionSyncService(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task<SyncResult> UpdateDataSync(UpdateSyncRequest request)
    {
        try
        {
            var existingTableReservation = await _db.TableReservation.FindAsync(request.ReservationId);
            if (existingTableReservation == null)
            {
                throw new NotFoundException($"Table reservation with ID {request.ReservationId} not found.");
            }

            existingTableReservation.ReservationStatus = request.ReservationStatus ?? existingTableReservation.ReservationStatus;
            existingTableReservation.EndTime = request.EndTime ?? existingTableReservation.EndTime;
            existingTableReservation.SeatedTime = request.SeatedTime ?? existingTableReservation.SeatedTime;
            existingTableReservation.TableNumber = request.TableNumber ?? existingTableReservation.TableNumber;

            await _db.SaveChangesAsync();

            return new SyncResult
            {
                Success = true,
                Message = "Table reservation updated successfully."
            };
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error during synchronization process: {ex.Message}");
        }
    }

}