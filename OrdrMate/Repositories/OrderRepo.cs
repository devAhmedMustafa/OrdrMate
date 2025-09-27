namespace OrdrMate.Repositories;

using OrdrMate.Models;
using OrdrMate.Data;
using Microsoft.EntityFrameworkCore;
using OrdrMate.Enums;
using OrdrMate.Utils.Exceptions;

public class OrderRepo : IOrderRepo
{
    private readonly OrdrMateDbContext _db;

    public OrderRepo(OrdrMateDbContext context)
    {
        _db = context;
    }

    public async Task<OrderIntent> CreateOrderIntent(OrderIntent orderIntent)
    {
        var savedOrderIntent = _db.OrderIntent.Add(orderIntent);
        await _db.SaveChangesAsync();
        return savedOrderIntent.Entity;
    }

    public async Task<OrderIntent?> GetOrderIntentById(string orderIntentId)
    {
        return await _db.OrderIntent
            .Include(o => o.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(oi => oi.Id == orderIntentId);
    }

    public async Task<OrderIntent?> UpdateOrderIntentStatus(string orderIntentId, PaymentStatus status)
    {
        var orderIntent = await _db.OrderIntent
            .FirstOrDefaultAsync(oi => oi.Id == orderIntentId);
        if (orderIntent == null)
        {
            throw new KeyNotFoundException($"OrderIntent with id {orderIntentId} not found.");
        }
        orderIntent.Status = status;
        _db.OrderIntent.Update(orderIntent);
        await _db.SaveChangesAsync();
        return orderIntent;
    }

    public async Task<Order> CreateOrder(Order order)
    {
        var savedOrder = _db.Order.Add(order);
        await _db.SaveChangesAsync();
        return savedOrder.Entity;
    }

    public async Task<Takeaway> CreateTakeawayOrder(Takeaway takeaway)
    {
        _db.Takeaway.Add(takeaway);
        await _db.SaveChangesAsync();
        return takeaway;
    }

    public async Task<OrderItem> CreateOrderItem(OrderItem orderItem)
    {
        var saved = _db.OrderItem.Add(orderItem);
        await _db.SaveChangesAsync();
        await _db.Entry(saved.Entity).Reference(oi => oi.Item).LoadAsync();
        await _db.Entry(saved.Entity).Reference(oi => oi.Order).LoadAsync();
        await _db.Entry(saved.Entity.Item!).Reference(i => i.Kitchen).LoadAsync();
        return saved.Entity;
    }

    public async Task<Order> GetDetailedOrderById(string orderId)
    {
        try
        {
            return await _db.Order
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Item)
                .Include(o => o.Branch).ThenInclude(b => b!.Restaurant)
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .Include(o => o.TableReservation)
                .Include(o => o.Takeaway)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error retrieving order with id {orderId}: {ex.Message}");
        }
    }

    public async Task<Takeaway?> GetTakeawayById(string orderId)
    {
        return await _db.Takeaway.FirstOrDefaultAsync(t => t.OrderId == orderId);
    }

    public async Task<IEnumerable<Takeaway>> GetTakeawaysByCustomerId(string customerId)
    {
        return await _db.Takeaway
            .Include(t => t.Order).ThenInclude(o => o!.Branch).ThenInclude(b => b!.Restaurant)
            .Include(t => t.Order).ThenInclude(o => o!.Customer)
            .Include(t => t.Order).ThenInclude(o => o!.Payment)
            .Where(t => t.Order!.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Takeaway>> GetAllTakeawaysByBranchId(string branchId)
    {
        return await _db.Takeaway
            .Include(t => t.Order).ThenInclude(o => o!.Branch).ThenInclude(b => b!.Restaurant)
            .Include(t => t.Order).ThenInclude(o => o!.Customer)
            .Include(t => t.Order).ThenInclude(o => o!.Payment)
            .Where(t => t.Order!.BranchId == branchId)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderById(string orderId)
    {
        return await _db.Order
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> SetOrderPaidStatus(string orderId, bool isPaid)
    {
        var order = await _db.Order
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {orderId} not found.");
        }
        order.IsPaid = isPaid;
        _db.Order.Update(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> SetOrderStatus(string orderId, OrderStatus status)
    {
        var order = await _db.Order
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {orderId} not found.");
        }

        order.Status = status;
        _db.Order.Update(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<IEnumerable<Order>> GetReadyOrdersByBranchId(string branchId)
    {
        return await _db.Order
            .Where(o => o.BranchId == branchId && o.Status == OrderStatus.Ready)
            .Include(o => o.Payment)
            .Include(o => o.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetUnpaidOrdersByBranchId(string branchId)
    {
        return await _db.Order
            .OrderByDescending(o => o.OrderDate)
            .OrderByDescending(o => o.OrderTime)
            .Where(o => o.BranchId == branchId && o.IsPaid == false)
            .Include(o => o.Payment)
            .Include(o => o.Customer)
            .Include(o => o.TableReservation)
            .Include(o => o.Takeaway)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersByBranchId(string branchId)
    {
        return await _db.Order
            .OrderByDescending(o => o.OrderDate)
            .OrderByDescending(o => o.OrderTime)
            .Where(o => o.BranchId == branchId)
            .Include(o => o.OrderItems!).ThenInclude(oi => oi.Item)
            .Include(o => o.Customer)
            .Include(o => o.TableReservation)
            .Include(o => o.Takeaway)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPaidOrdersOfBranch(string branchId)
    {
        return await _db.Order
            .Where(o => o.BranchId == branchId && o.IsPaid == true)
            .ToListAsync();
    }

    public async Task<bool> CancelOrderAsync(string orderId)
    {
        var order = await _db.Order
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Cancelled
        || order.Status == OrderStatus.InProgress
        || order.Status == OrderStatus.Ready)
        {
            return false;
        }

        await SetOrderStatus(orderId, OrderStatus.Cancelled);
        return true;
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerId(string customerId)
    {
        return await _db.Order
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Branch).ThenInclude(b => b!.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Payment)
            .Include(o => o.TableReservation)
            .Include(o => o.Takeaway)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrder(Order order)
    {
        try
        {
            var existingOrder = await _db.Order
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with id {order.Id} not found.");
            }

            _db.Entry(existingOrder).CurrentValues.SetValues(order);
            await _db.SaveChangesAsync();
            return existingOrder;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error updating order with id {order.Id}: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersWithinShift(string branchId, DateTime shiftStart, DateTime shiftEnd)
    {
        return await _db.Order
            .Where(o => o.BranchId == branchId &&
                        o.OrderDate >= shiftStart.Date && o.OrderDate <= shiftEnd.Date &&
                        (
                            (o.OrderDate > shiftStart.Date) ||
                            (o.OrderDate == shiftStart.Date && o.OrderTime >= TimeOnly.FromTimeSpan(shiftStart.TimeOfDay))
                        ) &&
                        (
                            (o.OrderDate < shiftEnd.Date) ||
                            (o.OrderDate == shiftEnd.Date && o.OrderTime <= TimeOnly.FromTimeSpan(shiftEnd.TimeOfDay))
                        )
            )
            .OrderByDescending(o => o.OrderDate)
            .Include(o => o.Customer)
            .Include(o => o.Payment)
            .Include(o => o.TableReservation)
            .Include(o => o.Takeaway)
            .Include(o => o.OrderItems!).ThenInclude(oi => oi.Item)
            .ToListAsync();
    }
}