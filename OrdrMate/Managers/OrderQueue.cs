namespace OrdrMate.Managers;

public class OrderQueue
{
    private readonly Queue<QueueItem> _items = new();

    private TimeOnly _startTime;

    public void AddItem(QueueItem items)
    {
        _items.Enqueue(items);

        if (_items.Count == 1)
        {
            ResetStartTime();
        }
    }

    public QueueItem Deque()
    {
        if (_items.Count > 0)
        {
            if (_items.Count > 0) ResetStartTime();
            return _items.Dequeue();
        }

        throw new InvalidOperationException("Queue is empty, cannot dequeue.");
    }

    public QueueItem? GetPeekItem()
    {
        if (_items.Count == 0)
        {
            return null;
        }

        return _items.Peek();
    }

    public void ResetStartTime()
    {
        _startTime = TimeOnly.FromDateTime(DateTime.Now);
    }

    public bool IsOrderInProcess(string orderId)
    {
        if (_items.Count == 0)
        {
            return false;
        }

        return _items.Peek().OrderId == orderId;
    }

    public bool Contains(string orderId)
    {
        if (_items.Count == 0) return false;
        return _items.Any(item => item.OrderId == orderId);
    }

    public decimal GetEstimatedTime()
    {
        if (_items.Count == 0)
        {
            return 0.0m;
        }
        decimal totalTime = 0.0m;
        foreach (var item in _items)
        {
            totalTime += item.PreparationTime * item.Quantity;
        }

        return totalTime;
    }

    public decimal GetEstimatedTimeForOrder(string orderId)
    {
        if (_items.Count == 0)
        {
            return 0.0m;
        }

        decimal totalTime = 0.0m;
        var orderTriggered = 0;

        foreach (var item in _items)
        {
            if (item.OrderId == orderId)
            {
                orderTriggered++;
            }
            else
            {
                if (orderTriggered > 0)
                {
                    return totalTime;
                }
            }

            totalTime += item.PreparationTime * item.Quantity;
        }

        if (orderTriggered == 0) return 0.0m;

        var timeSinceStart = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan() - _startTime.ToTimeSpan();
        if (timeSinceStart.TotalSeconds > 0)
        {
            totalTime += (decimal)timeSinceStart.TotalMinutes;
        }

        return totalTime;
    }

    public int Count => _items.Count;
    public bool IsEmpty => _items.Count == 0;
    public List<QueueItem> PeekAllItems()
    {
        return [.. _items];
    }
}

public record QueueItem
{
    public required string OrderId { get; set; }
    public required DateTime OrderDate { get; set; }
    public required string ItemName { get; set; }
    public required string KitchenName { get; set; }
    public int KitchenUnitId { get; set; } = 0;
    public required string ItemId { get; set; }
    public required string ImageUrl { get; set; }
    public required decimal Price { get; set; }
    public required decimal PreparationTime { get; set; }
    public required int Quantity { get; set; }
}