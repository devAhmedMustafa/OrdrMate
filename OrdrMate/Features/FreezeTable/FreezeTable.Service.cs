using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.FreezeTable;

public class FreezeTableService
{

    private readonly ITableRepo _tableRepo;
    private readonly FreezeTableRepo _freezeTableRepo;

    public FreezeTableService(ITableRepo tableRepo, FreezeTableRepo freezeTableRepo)
    {
        _tableRepo = tableRepo;
        _freezeTableRepo = freezeTableRepo;
    }

    public async Task<bool> FreezeTable(string branchId, int tableNumber)
    {
        try
        {
            var table = await _tableRepo.GetTableByNumber(branchId, tableNumber);
            if (table == null) return false;
            table.IsFrozen = true;
            await _tableRepo.UpdateTable(table);
            return true;
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to freeze table: {ex.Message}");
        }
    }

    public async Task<bool> UnfreezeTable(string branchId, int tableNumber)
    {
        try
        {
            var table = await _tableRepo.GetTableByNumber(branchId, tableNumber);
            if (table == null) return false;
            table.IsFrozen = false;
            await _tableRepo.UpdateTable(branchId, tableNumber, table);
            return true;
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to unfreeze table: {ex.Message}");
        }
    }

    public async Task<bool> IsTableFrozen(string branchId, int tableNumber)
    {
        try
        {
            return await _freezeTableRepo.IsTableFrozen(branchId, tableNumber);
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to check if table is frozen: {ex.Message}");
        }
    }
}