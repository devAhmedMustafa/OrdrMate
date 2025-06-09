using System.Text.Json;
using OrdrMate.DTOs.Order;
using OrdrMate.Enums;
using OrdrMate.Events;
using OrdrMate.Models;

namespace OrdrMate.Managers;

public class RestaurantQueueManager
{
    private readonly Branch _restaurant;
    private readonly HashSet<string> _orderIds = [];
    private readonly Dictionary<string, List<OrderQueue>> _restaurantQueues = [];

    public RestaurantQueueManager(Branch restaurant)
    {
        _restaurant = restaurant;

        if (restaurant.KitchenPowers != null)
        {
            foreach (var kp in restaurant.KitchenPowers)
            {
                if (kp.Kitchen == null) continue;

                _restaurantQueues[kp.Kitchen.Name] = [];
                for (int i = 0; i < kp.Units; i++)
                {
                    _restaurantQueues[kp.Kitchen.Name][i] = new OrderQueue();
                }
            }
        }

        if (restaurant.Orders != null)
        {
            foreach (var order in restaurant.Orders)
            {
                if (order.Status != OrderStatus.Queued) continue;
                if (order.OrderItems == null) continue;
                foreach (var orderItem in order.OrderItems)
                {
                    var kitchenName = orderItem.Item?.Kitchen?.Name;
                    AddItemToQueue(kitchenName!, new QueueItem
                    {
                        OrderId = order.Id,
                        OrderDate = order.OrderDate,
                        ItemName = orderItem.Item?.Name ?? "Unknown Item",
                        Price = orderItem.Price,
                        Quantity = orderItem.Quantity,
                        ItemId = orderItem.ItemId,
                        KitchenName = kitchenName ?? "Unknown Kitchen",
                        ImageUrl = orderItem.Item?.ImageUrl ?? string.Empty,
                        PreparationTime = orderItem.Item?.PreperationTime ?? 0.0m,
                    });
                }
            }
        }
    }

    public void UpdateKitchen(string kitchenName, int units)
    {
        if (!_restaurantQueues.ContainsKey(kitchenName))
        {
            _restaurantQueues[kitchenName] = [];
        }

        for (int i = _restaurantQueues[kitchenName].Count; i < units; i++)
        {
            _restaurantQueues[kitchenName][i] = new OrderQueue();
        }
    }

    public void AddItemToQueue(string kitchen, QueueItem order)
    {
        var kitchenQueues = _restaurantQueues.GetValueOrDefault(kitchen);
        if (kitchenQueues == null) return;

        var bestQueue = -1;
        var minLength = int.MaxValue;

        for (int i = 0; i < kitchenQueues.Count; i++)
        {
            var queue = kitchenQueues[i];
            if (queue.Count < minLength)
            {
                minLength = queue.Count;
                bestQueue = i;
            }
        }

        if (bestQueue >= 0)
        {
            kitchenQueues[bestQueue].AddItem(order);
        }

        order.KitchenUnitId = bestQueue;
        _orderIds.Add(order.OrderId);

        OrderEvents.OnItemInQueue(_restaurant.Id, order.OrderId, order.ItemId);
    }

    public QueueItem? GetNextItem(string kitchen, int kitchenIdx)
    {
        return _restaurantQueues[kitchen]?[kitchenIdx]?.GetPeekItem();
    }

    public void DequeueItem(string kitchen, int kitchenIdx)
    {
        _restaurantQueues[kitchen]?[kitchenIdx]?.Deque();
    }

    public KeyValuePair<string, int>[] GetAllKitchens()
    {
        return [.. _restaurantQueues.Select(kv => new KeyValuePair<string, int>(kv.Key, kv.Value.Count))];
    }

    public List<QueueItem> PeekAllItems()
    {
        return [.. _restaurantQueues.SelectMany(kv => kv.Value.SelectMany(queue => queue.PeekAllItems()))];
    }

    public bool IsOrderInProcess(string orderId)
    {
        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                if (queue.IsOrderInProcess(orderId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsOrderInQueue(string orderId)
    {
        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                if (queue.Contains(orderId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public OrderStatus GetOrderStatus(string orderId)
    {
        if (IsOrderInProcess(orderId)) return OrderStatus.InProgress;
        if (IsOrderInQueue(orderId)) return OrderStatus.Queued;

        return OrderStatus.Ready;

    }

    public void CleanupFinishedOrderIds()
    {
        _orderIds.RemoveWhere(orderId =>
        {
            var isReady = !IsOrderInQueue(orderId) && !IsOrderInProcess(orderId);
            if (isReady) OrderEvents.OnOrderReady(orderId);
            return isReady;
        }
        );
    }

    public JsonElement GetRestaurantInfo()
    {
        var orderList = new List<JsonElement>();
        foreach (var order in _orderIds)
        {
            var orderStatus = GetOrderStatus(order);
            var orderInfo = new
            {
                OrderId = order,
                Status = orderStatus,
            };

            orderList.Add(JsonSerializer.SerializeToElement(orderInfo));
        }

        return JsonSerializer.SerializeToElement(new
        {
            orderList
        });
    }

    public decimal GetMinWaitingTime()
    {
        decimal minTime = decimal.MaxValue;

        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                var estimatedTime = queue.GetEstimatedTime();
                if (estimatedTime < minTime)
                {
                    minTime = estimatedTime;
                }
            }
        }

        return minTime == decimal.MaxValue ? 0.0m : minTime;
    }

    public decimal GetMaxWaitingTime()
    {
        decimal maxTime = 0.0m;

        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                var estimatedTime = queue.GetEstimatedTime();
                if (estimatedTime > maxTime)
                {
                    maxTime = estimatedTime;
                }
            }
        }

        return maxTime;
    }

    public decimal GetAverageWaitingTime()
    {
        decimal totalTime = 0.0m;
        int count = 0;
        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                totalTime += queue.GetEstimatedTime();
                count += queue.Count;
            }
        }
        return count == 0 ? 0.0m : totalTime / count;
    }

    public OrderWaitingTimesDto GetEstimatedTimes()
    {
        return new OrderWaitingTimesDto
        {
            MinWaitingTime = GetMinWaitingTime(),
            MaxWaitingTime = GetMaxWaitingTime(),
            AverageWaitingTime = GetAverageWaitingTime()
        };
    }

    public decimal GetEstimatedTimeForOrder(string orderId)
    {
        var maxTime = 0.0m;

        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                var estimatedTime = queue.GetEstimatedTimeForOrder(orderId);
                if (estimatedTime > maxTime)
                {
                    maxTime = estimatedTime;
                }
            }
        }

        return maxTime;
    }
 
    public List<QueueItem> GetItemQueues()
    {
        var allItems = new List<QueueItem>();
        foreach (var kitchen in _restaurantQueues.Values)
        {
            foreach (var queue in kitchen)
            {
                allItems.AddRange(queue.PeekAllItems());
            }
        }

        return allItems;
    }

}