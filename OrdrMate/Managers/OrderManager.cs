using OrdrMate.Events;
using OrdrMate.Repositories;
using OrdrMate.Models;
using System.Text.Json;
using OrdrMate.Sockets;
using OrdrMate.DTOs.Order;
using OrdrMate.Services;

namespace OrdrMate.Managers;

public class OrderManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBranchRepo _branchRepo;
    private readonly BranchSocketHandler _branchOrdersSocketHandler;
    public static readonly Dictionary<string, RestaurantQueueManager> restaurantManagers = [];
    private static bool _initialized = false;

    public OrderManager(
        IBranchRepo branchRepo,
        BranchSocketHandler branchOrdersSocketHandler,
        IServiceScopeFactory scopeFactory
        )
    {
        _scopeFactory = scopeFactory;
        _branchRepo = branchRepo;
        _branchOrdersSocketHandler = branchOrdersSocketHandler;

        if (_initialized) return;

        Init();

        BranchEvents.BranchCreated += OnBranchCreated;
        BranchEvents.BranchDeleted += OnBranchDeleted;
        OrderEvents.OrderPlaced += OnOrderPlaced;
        OrderEvents.OrderReady += OnOrderReady;
        BranchEvents.KitchenUpdate += OnKitchenUpdate;

        _initialized = true;
    }


    private void Init()
    {
        var restaurants = _branchRepo.GetAllBranches().Result;
        foreach (var restaurant in restaurants)
        {
            restaurantManagers[restaurant.Id] = new RestaurantQueueManager(restaurant);
        }
    }

    private void OnBranchCreated(Branch branch)
    {
        restaurantManagers[branch.Id] = new RestaurantQueueManager(branch);
    }

    private void OnBranchDeleted(Branch branch)
    {
        restaurantManagers.Remove(branch.Id);
    }

    private async void OnOrderPlaced(string branchId, List<OrderItem> orderItems)
    {
        Console.WriteLine($"Order placed for branch {branchId} with items: {string.Join(", ", orderItems.Select(oi => oi?.Item?.Name ?? "Unknown"))}");

        var restaurantManager = restaurantManagers[branchId];

        if (restaurantManager == null)
        {
            restaurantManagers[branchId] = new RestaurantQueueManager(_branchRepo.GetDetailedBranchById(branchId).Result);
            restaurantManager = restaurantManagers[branchId];
        }

        foreach (var oi in orderItems)
        {
            if (restaurantManager == null) continue;

            var kitchenName = oi.Item?.Kitchen?.Name;

            if (kitchenName == null) continue;

            var item = new QueueItem
            {
                OrderId = oi.OrderId,
                OrderDate = DateTime.Now,
                ItemName = oi.Item?.Name ?? "Unknown",
                Quantity = oi.Quantity,
                Price = oi.Price,
                ItemId = oi.ItemId,
                KitchenName = kitchenName,
                ImageUrl = oi.Item?.ImageUrl ?? string.Empty,
                PreparationTime = oi.Item?.PreperationTime ?? 0.0m,
            };

            Console.WriteLine($"Adding item to queue: {item.ItemName}, OrderId: {item.OrderId}, Kitchen: {kitchenName}");
            restaurantManager.AddItemToQueue(kitchenName, item);

            // Expose to socket clients

            var json = JsonSerializer.Serialize(new
            {
                Type = "NewItem",
                Item = item
            });

            await _branchOrdersSocketHandler.SendToBranch(branchId, json);
        }

        var jsonOrder = JsonSerializer.Serialize(new
        {
            Type = "OrderPlaced",
            orderItems[0].OrderId,
            IsBeingPrepared = restaurantManager?.IsOrderInProcess(orderItems[0].OrderId),
        });

        await _branchOrdersSocketHandler.SendToBranch(branchId, jsonOrder);

    }

    public void CheckPreparedInQueue(string branchId, string kitchenName, int kitchenUnitId)
    {
        try
        {
            restaurantManagers[branchId].DequeueItem(kitchenName, kitchenUnitId);
            restaurantManagers[branchId].CleanupFinishedOrderIds();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking prepared items in queue for branch {branchId}, kitchen {kitchenName}, unit {kitchenUnitId}: {ex.Message}", ex);
        }
    }

    public void OrdersStatus(string branchId)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                Type = "OrderStatus",
                Orders = restaurantManagers[branchId].GetRestaurantInfo()
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching order status for branch {branchId}: {ex.Message}", ex);
        }
    }

    public void OnKitchenUpdate(string branchId, string kitchenName, int units)
    {
        try
        {
            if (!restaurantManagers.ContainsKey(branchId))
            {
                restaurantManagers[branchId] = new RestaurantQueueManager(_branchRepo.GetDetailedBranchById(branchId).Result);
            }

            restaurantManagers[branchId].UpdateKitchen(kitchenName, units);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating kitchen for branch {branchId}: {ex.Message}", ex);
        }
    }

    public Task<OrderWaitingTimesDto> GetEstimatedTimes(string branchId)
    {
        try
        {
            var estimatedTimes = restaurantManagers[branchId].GetEstimatedTimes();
            return Task.FromResult(estimatedTimes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching estimated times for branch {branchId}: {ex.Message}", ex);
        }
    }

    public Task<decimal> GetEstimatedTimeForOrder(string branchId, string orderId)
    {
        try
        {
            var estimatedTime = restaurantManagers[branchId].GetEstimatedTimeForOrder(orderId);
            return Task.FromResult(estimatedTime);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching estimated time for order {orderId} in branch {branchId}: {ex.Message}", ex);
        }
    }

    public async void OnOrderReady(string orderId)
    {

        using var scope = _scopeFactory.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();

        Console.WriteLine($"Order {orderId} is ready.");

        var order = await orderRepo.SetOrderStatus(orderId, Enums.OrderStatus.Ready);
        if (order == null)
        {
            Console.WriteLine($"Order with ID {orderId} not found or could not be updated.");
            return;
        }

        var branchId = order.BranchId;
        var json = JsonSerializer.Serialize(new
        {
            Type = "OrderReady",
            OrderId = order.Id,
        });

        try
        {
            var cloudMessaging = scope.ServiceProvider.GetRequiredService<CloudMessaging>();
            var firebaseToken = await cloudMessaging.GetTokenByUserId(order.CustomerId);
            
            if (string.IsNullOrEmpty(firebaseToken))
            {
                Console.WriteLine($"No Firebase token found for customer with ID {order.CustomerId}.");
                return;
            }
            await cloudMessaging.SendNotificationAsync(
                firebaseToken,
                "Your order is ready!",
                $"Order #{order.Id} is ready for pickup at {order.Branch?.Restaurant?.Name}."
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification for order {orderId}: {ex.Message}");
        }

        await _branchOrdersSocketHandler.SendToBranch(branchId, json);

    }

    public List<QueueItem> GetItemQueues(string branchId)
    {
        if (!restaurantManagers.TryGetValue(branchId, out var restaurantManager))
        {
            throw new KeyNotFoundException($"No restaurant manager found for branch {branchId}.");
        }

        var itemQueues = restaurantManager.GetItemQueues();
        if (itemQueues == null)
        {
            throw new Exception($"No item queues found for branch {branchId}.");
        }

        return itemQueues;
    }
}