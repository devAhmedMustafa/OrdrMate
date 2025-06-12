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
    Task<Order?> GetTableOrderByReservationId(string reservationId);
    Task<IEnumerable<TableReservation>> GetTableReservationsByCustomerId(string customerId);
    Task<TableReservation?> GetTableReservationByOrderId(string orderId);
    Task<TableReservation?> GetTableReservationById(string reservationId);
    Task<IEnumerable<TableReservation>> GetTableReservationsInQueue(string branchId, int tableNumber);
}