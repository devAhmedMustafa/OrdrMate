namespace OrdrMate.Repositories;

using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

public class PaymentRepo : IPaymentRepo
{
    private readonly OrdrMateDbContext _db;
    public PaymentRepo(OrdrMateDbContext context)
    {
        _db = context;
    }
    public async Task<Payment> CreatePayment(Payment payment)
    {
        _db.Payment.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public Task<Payment?> GetPaymentById(string paymentId)
    {
        return _db.Payment
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException($"Payment with id {paymentId} not found.");
    }

    public Task<Payment?> GetPaymentByOrderId(string orderId)
    {
        return _db.Payment
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public Task<Payment> UpdatePayment(Payment payment)
    {
        _db.Payment.Update(payment);
        return _db.SaveChangesAsync().ContinueWith(_ => payment);
    }
}