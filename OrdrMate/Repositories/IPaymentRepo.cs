namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface IPaymentRepo
{
    public Task<Payment> CreatePayment(Payment payment);
    public Task<Payment> UpdatePayment(Payment payment);
    public Task<Payment?> GetPaymentById(string paymentId);
    public Task<Payment?> GetPaymentByOrderId(string orderId);
}