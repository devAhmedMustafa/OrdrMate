using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;

namespace OrdrMate.Features.Orders.Delivery;

public class DeliveryRepo
{
    
    private readonly OrdrMateDbContext _db;

    public DeliveryRepo(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task<Delivery?> GetDeliveryByOrderId(string orderId)
    {
        var delivery = await _db.Delivery
        .Include(d => d.Order)
        .ThenInclude(o => o!.Branch)
        .ThenInclude(b => b!.Restaurant)
        .FirstOrDefaultAsync(d => d.OrderId == orderId);
        
        return delivery;
    }

    public async Task<Delivery> CreateDeliveryOrder(Delivery delivery)
    {
        await _db.Delivery.AddAsync(delivery);
        await _db.SaveChangesAsync();
        return delivery;
    }

    public async Task UpdateDelivery(Delivery delivery)
    {
        _db.Delivery.Update(delivery);
        await  _db.SaveChangesAsync();
    }

}