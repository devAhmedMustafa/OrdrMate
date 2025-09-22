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

    public async Task<TableReservationDto> ReserveTable(OrderDto order, int tableNumber)
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
                OrderId = order.OrderId,
                ReservationTime = DateTime.UtcNow,
                TableNumber = tableNumber,
            };

            await _tableManager.ReserveTable(tableNumber, reservation);

            reservation.TableNumber = tableNumber;

            return new TableReservationDto
            {
                TableNumber = tableNumber,
            };
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
            Order = r.Order
        });
    }

    public async Task<TableReservationDto?> GetTableReservationByOrderId(string orderId)
    {
        var reservation = await _tableRepo.GetTableReservationByOrderId(orderId);
        if (reservation == null) return null;

        return new TableReservationDto
        {
            TableNumber = reservation.TableNumber,
            Order = reservation.Order,
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
            OrderId = r.OrderId,
            OrderStatus = r.Order?.Status.ToString() ?? "Unknown"
        });
    }

    public async Task<OrderDto?> GetOrderByTableReservationId(string reservationId)
    {
        var order = await _tableRepo.GetTableOrderByReservationId(reservationId);

        if (order == null) throw new Exception("Order not found");

        return new OrderDto
        {
            OrderId = order.Id,
            BranchId = order.BranchId,
            CustomerId = order.CustomerId,
            OrderStatus = order.Status.ToString(),
            OrderType = order.OrderType.ToString(),
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            OrderItems = [.. order.OrderItems!.Select(oi => new OrderItemDto
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
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown",
            Customer = order.Customer?.Username ?? "Unknown",
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unknown",
        };
    }

}