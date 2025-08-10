
using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Enums;
using OrdrMate.Models;
using OrdrMate.Repositories;

public class CancelOrderAsyncTests
{
    private readonly OrderRepo _OrderRepo;
    private readonly OrdrMateDbContext _context;

    public CancelOrderAsyncTests()
    {
        var options = new DbContextOptionsBuilder<OrdrMateDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new OrdrMateDbContext(options);
        _OrderRepo = new OrderRepo(_context);
    }
    [Fact]
    public async Task CancelOrderAsync_OrderDoesNotExist()
    {
        var orderId = "999";

        var result = await _OrderRepo.CancelOrderAsync(orderId);

        Assert.False(result);
    }
    [Fact]
    public async Task CancelOrderAsync_OrderAlreadyCancelld()
    {
        var order = new Order { Id = "123", Status = OrderStatus.Cancelled };
        _context.Order.Add(order);
        await _context.SaveChangesAsync();

        var result = await _OrderRepo.CancelOrderAsync(order.Id);

        Assert.False(result);
    }
    [Fact]
    public async Task CancelOrderAsync_OrderNotQueued()
    {
        var order = new Order { Id = "456", Status = OrderStatus.InProgress };
        _context.Order.Add(order);
        await _context.SaveChangesAsync();

        var result = await _OrderRepo.CancelOrderAsync(order.Id);

        Assert.False(result);
    }
    [Fact]
    public async Task CancelOrderAsync_QueuedOrder()
    {
        var order = new Order { Id = "789", Status = OrderStatus.Queued };
        _context.Order.Add(order);
        await _context.SaveChangesAsync();

        var result = await _OrderRepo.CancelOrderAsync(order.Id);

        Assert.True(result);
        var updatedOrder = await _context.Order.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Cancelled, updatedOrder.Status);
    }
    [Fact]
    public async Task CancelOrderAsync_NullOrderId()
    {
        string orderId = "";

        var result = await _OrderRepo.CancelOrderAsync(orderId);

        Assert.False(result);
        }
   
}