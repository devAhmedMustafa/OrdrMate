using OrdrMate.Enums;

namespace OrdrMate.Models;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string BranchId { get; set; }
    public required string CustomerId { get; set; }
    public TimeOnly OrderTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Queued;
    public required decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; } = false;
    public Branch? Branch { get; set; }
    public User? Customer { get; set; }
    public Payment? Payment { get; set; }
    public DateTime ReadyTime { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
}