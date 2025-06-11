using OrdrMate.Events;
using OrdrMate.Repositories;
using OrdrMate.Models;
using System.Text.Json;
using OrdrMate.Sockets;
using OrdrMate.DTOs.Order;
using OrdrMate.Services;
using OrdrMate.DTOs.Item;

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
        OrderEvents.OrderInProgress += OnOrderInProgress;

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
            var kitchenName = oi.Item?.Kitchen?.Name;

            if (kitchenName == null)
            {
                Console.WriteLine($"Item {oi.Item?.Name ?? "Unknown"} does not have a kitchen assigned. Skipping item.");
                continue;
            }

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
            if (!restaurantManager.AddItemToQueue(kitchenName, item))
            {
                Console.WriteLine($"Failed to add item {item.ItemName} to queue for kitchen {kitchenName} in branch {branchId}.");
                continue;
            }


            var json = JsonSerializer.Serialize(new
            {
                Type = "NewItem",
                Item = item
            });

            await _branchOrdersSocketHandler.SendToBranch(branchId, json);
        }

        var orderRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IOrderRepo>();
        var order = await orderRepo.SetOrderStatus(orderItems[0].OrderId, Enums.OrderStatus.Queued);

        var jsonOrder = JsonSerializer.Serialize(new
        {
            Type = "OrderPlaced",
            orderItems[0].OrderId,
            IsBeingPrepared = restaurantManager?.IsOrderInProcess(orderItems[0].OrderId),
        });

        await _branchOrdersSocketHandler.SendToBranch(branchId, jsonOrder);

    }

    public NextInQueueDto CheckPreparedInQueue(string branchId, string kitchenName, int kitchenUnitId)
    {
        try
        {
            var itemDequed = restaurantManagers[branchId].DequeueItem(kitchenName, kitchenUnitId);

            if (itemDequed == null) throw new Exception($"No items found in queue for branch {branchId}, kitchen {kitchenName}, unit {kitchenUnitId}.");

            restaurantManagers[branchId].CleanupFinishedOrderIds();

            var nextItem = restaurantManagers[branchId].GetNextItem(kitchenName, kitchenUnitId);

            return new NextInQueueDto
            {
                DequeudItemId = itemDequed.ItemId,
                NextItemId = nextItem?.ItemId,
                KitchenName = kitchenName,
                KitchenUnit = kitchenUnitId,
            };
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

    public async void OnOrderInProgress(string orderId)
    {
        using var scope = _scopeFactory.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();

        Console.WriteLine($"Order {orderId} is in progress.");
        var branchId = restaurantManagers.FirstOrDefault(x => x.Value.IsOrderInProcess(orderId)).Key;

        if (branchId == null)
        {
            Console.WriteLine($"No branch found for order {orderId}.");
            return;
        }

        var order = await orderRepo.SetOrderStatus(orderId, Enums.OrderStatus.InProgress);


        var json = JsonSerializer.Serialize(new
        {
            Type = "OrderInProgress",
            OrderId = orderId,
        });

        try
        {
            var cloudMessaging = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CloudMessaging>();

            if (order == null)
            {
                Console.WriteLine($"Order with ID {orderId} not found.");
                return;
            }
            var firebaseToken = cloudMessaging.GetTokenByUserId(order.CustomerId).Result;

            if (string.IsNullOrEmpty(firebaseToken))
            {
                Console.WriteLine($"No Firebase token found for customer with ID {order.CustomerId}.");
                return;
            }
            cloudMessaging.SendNotificationAsync(
                firebaseToken,
                "Your order is in progress!",
                $"Order is being prepared at {order.Branch?.Restaurant?.Name}."
            ).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification for order {orderId}: {ex.Message}");
        }

        _branchOrdersSocketHandler.SendToBranch(branchId, json).Wait();
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