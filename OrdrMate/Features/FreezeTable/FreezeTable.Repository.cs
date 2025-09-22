using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.FreezeTable;

public class FreezeTableRepo
{
    private readonly OrdrMateDbContext _db;

    public FreezeTableRepo(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsTableFrozen(string branchId, int tableNumber)
    {
        try
        {
            var freezeEntry = await _db.Table
            .Select(t => new { t.IsFrozen, t.BranchId, t.TableNumber })
            .FirstOrDefaultAsync(t => t.BranchId == branchId && t.TableNumber == tableNumber);

            return freezeEntry != null && freezeEntry.IsFrozen;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to check if table is frozen: {ex.Message}");
        }
    }
}