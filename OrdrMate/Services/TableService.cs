namespace OrdrMate.Services;

using OrdrMate.DTOs.Item;
using OrdrMate.DTOs.Order;
using OrdrMate.DTOs.Table;
using OrdrMate.Features.FreezeTable;
using OrdrMate.Managers;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

public class TableService
{
    private readonly ITableRepo _tableRepo;
    private readonly TableManager _tableManager;
    private readonly FreezeTableService _freezeTableService;

    public TableService(ITableRepo tableRepo, TableManager tableManager, FreezeTableService freezeTableService)
    {
        _tableRepo = tableRepo;
        _tableManager = tableManager;
        _freezeTableService = freezeTableService;
    }

    public async Task<IEnumerable<TableDto>> GetAllTablesOfBranch(string branchId)
    {
        var tables = await _tableRepo.GetAllTablesOfBranch(branchId);
        return tables.Select(t => new TableDto
        {
            TableNumber = t.TableNumber,
            Seats = t.Seats,
            BranchId = t.BranchId,
            ReservationCount = _tableManager.GetReservationCount(branchId, t.TableNumber),
            IsFrozen = t.IsFrozen
        });
    }

    public async Task<TableDto> CreateTable(AddTableDto tableDto)
    {
        var table = new Table
        {
            TableNumber = tableDto.TableNumber,
            Seats = tableDto.Seats,
            BranchId = tableDto.BranchId
        };

        var createdTable = await _tableRepo.CreateTable(table);
        return new TableDto
        {
            TableNumber = createdTable.TableNumber,
            Seats = createdTable.Seats,
            BranchId = createdTable.BranchId,
            IsFrozen = createdTable.IsFrozen
        };
    }

    public async Task<bool> DeleteTable(string branchId, int tableNum)
    {
        return await _tableRepo.DeleteTable(branchId, tableNum);
    }

    public async Task<TableReservation> ReserveTable(OrderDto order, int tableNumber)
    {
        try
        {
            var isTableFrozen = await _freezeTableService.IsTableFrozen(order.BranchId, tableNumber);
            if (isTableFrozen)
            {
                throw new ForbidException("Table is currently frozen and cannot be reserved.");
            }

            var reservation = new TableReservation
            {
                BranchId = order.BranchId,
                CustomerId = order.CustomerId,
                ReservationTime = DateTime.UtcNow,
                TableNumber = tableNumber,
            };

            await _tableManager.ReserveTable(tableNumber, reservation);
            return reservation;
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to reserve table: {ex.Message}");
        }
    }

    public async Task<int> GetFreeTableCount(string branchId)
    {
        var reservations = await _tableRepo.GetTableReservationsByBranchId(branchId);
        var reservedTables = reservations
            .Where(r => r.ReservationStatus == "Queued")
            .Select(r => r.TableNumber)
            .Distinct()
            .ToHashSet();

        var allTables = await _tableRepo.GetAllTablesOfBranch(branchId);
        return allTables.Count(t => !reservedTables.Contains(t.TableNumber));
    }

    public async Task<IEnumerable<TableReservationDto>> GetCustomerTableReservation(string customerId)
    {
        var reservations = await _tableRepo.GetTableReservationsByCustomerId(customerId);
        return reservations.Select(r => new TableReservationDto
        {
            TableNumber = r.TableNumber,
            Orders = r.Orders
        });
    }

    public async Task<TableReservationDto?> GetTableReservationByOrderId(string orderId)
    {
        var reservation = await _tableRepo.GetTableReservationByOrderId(orderId);
        if (reservation == null) return null;

        return new TableReservationDto
        {
            TableNumber = reservation.TableNumber,
            Orders = reservation.Orders,
            ReservationTime = reservation.ReservationTime
        };
    }

    public async Task<IEnumerable<TableReservationResponseDto>> GetTableReservationsInQueue(string branchId, int tableNumber)
    {
        var reservations = await _tableRepo.GetTableReservationsInQueue(branchId, tableNumber);
        return reservations.Select(r => new TableReservationResponseDto
        {
            ReservationId = r.ReservationId,
            TableNumber = r.TableNumber,
            CustomerName = r.Customer?.Username ?? "Unknown",
            ReservationDate = r.ReservationTime,
            ReservationStatus = r.ReservationStatus,
        });
    }

    public async Task<OrderDto?> GetOrderByTableReservationId(string reservationId)
    {

        var reservation = await _tableRepo.GetTableReservationById(reservationId);
        if (reservation == null)
        {
            throw new NotFoundException("Reservation not found");
        }

        var orders = await _tableRepo.GetTableOrdersByReservationId(reservationId);

        if (orders == null) throw new Exception("Orders not found");

        return new OrderDto
        {
            OrderId = orders.First().Id,
            BranchId = reservation.BranchId,
            CustomerId = reservation.CustomerId,
            OrderStatus = "Unknown",
            OrderType = orders.First().OrderType.ToString(),
            OrderDate = orders.First().OrderDate,
            TotalAmount = orders.First().TotalAmount,
            OrderItems = [.. orders.SelectMany(o => o.OrderItems!).Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Item = new ItemDto
                {
                    Name = oi.Item?.Name ?? "Unknown",
                    Description = oi.Item?.Description ?? "Unknown",
                    Price = oi.Item?.Price ?? 0,
                    PreparationTime = oi.Item?.PreperationTime ?? 0,
                    KitchenName = oi.Item?.Kitchen?.Name ?? "Unknown",
                    Category = oi.Item?.CategoryName ?? "Unknown"
                }
            })],
            RestaurantName = reservation.Branch?.Restaurant?.Name ?? "Unknown",
            Customer = reservation.Customer?.Username ?? "Unknown",
            PaymentMethod = "Unknown",
        };
    }
}