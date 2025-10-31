namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface ITableRepo
{
    Task<IEnumerable<Table>> GetAllTablesOfBranch(string branchId);
    Task<Table> CreateTable(Table table);
    Task<Table> UpdateTable(string branchId, int tableNum, Table table);
    Task<bool> DeleteTable(string branchId, int tableNum);
    Task<TableReservation?> CreateTableReservation(TableReservation reservation);
    Task<IEnumerable<TableReservation>> GetTableReservationsByBranchId(string branchId);
    Task<TableReservation> UpdateTableReservationStatus(string reservationId, string status);
    Task<IEnumerable<Order>> GetTableOrdersByReservationId(string reservationId);
    Task<IEnumerable<TableReservation>> GetTableReservationsByCustomerId(string customerId);
    Task<TableReservation?> GetTableReservationByOrderId(string orderId);
    Task<TableReservation?> GetTableReservationById(string reservationId);
    Task<IEnumerable<TableReservation>> GetTableReservationsInQueue(string branchId, int tableNumber);
    Task<Table?> GetTableByNumber(string branchId, int tableNumber);
    Task<bool> UpdateTableReservationTableNumber(string reservationId, int newTableNumber);
    Task<Table> UpdateTable(Table table);
    Task<Features.Orders.TableReservation?> CreateTableReservation(Features.Orders.TableReservation tableReservation);
}