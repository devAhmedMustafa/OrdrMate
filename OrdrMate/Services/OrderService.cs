using OrdrMate.Enums;
using OrdrMate.DTOs.Order;
using OrdrMate.DTOs.Item;
using OrdrMate.Models;
using OrdrMate.Utils;
using OrdrMate.Repositories;
using OrdrMate.Events;
using Hangfire;
using OrdrMate.Features.BestBranchToOrder;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Mappers.Orders;

namespace OrdrMate.Services;

public class OrderService
{
    private readonly PaymentService _paymentService;
    private readonly IOrderRepo _orderRepo;
    private readonly IDeliverRequestRepo _deliverRequestRepo;
    private readonly PaymobService _paymobService;
    private readonly CloudMessaging _cloudMessaging;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly BestBranchToOrderService _bestBranchService;
    private readonly ItemAvailabilityService _itemAvailabilityService;

    private static readonly Dictionary<string, string> _jobIds = new Dictionary<string, string>();

    public OrderService(
        PaymentService paymentService,
        IOrderRepo orderRepo,
        IDeliverRequestRepo deliverRequestRepo,
        PaymobService paymobService,
        CloudMessaging cloudMessaging,
        IBackgroundJobClient backgroundJobs,
        BestBranchToOrderService bestBranchService,
        ItemAvailabilityService itemAvailabilityService
    )
    {
        _paymentService = paymentService;
        _orderRepo = orderRepo;
        _deliverRequestRepo = deliverRequestRepo;
        _paymobService = paymobService;
        _cloudMessaging = cloudMessaging;
        _backgroundJobs = backgroundJobs;
        _bestBranchService = bestBranchService;
        _itemAvailabilityService = itemAvailabilityService;
    }

    public async Task<OrderIntentDto> CreateOrderIntent(PlaceOrderDto placeOrderDto)
    {
        try
        {
            var bestBranchId = await _bestBranchService.FindBestBranchToOrder(placeOrderDto);

            var totalAmount = placeOrderDto.Items.Sum(oi => oi.Price * oi.Quantity);

            var intent = new OrderIntent
            {
                CustomerId = placeOrderDto.CustomerId,
                CustomerName = placeOrderDto.CustomerName,
                CustomerPhone = placeOrderDto.CustomerPhone,
                BranchId = bestBranchId,
                Status = PaymentStatus.INITIATED,
                Amount = totalAmount,
                PaymentMethod = placeOrderDto.PaymentMethod,
                OrderType = placeOrderDto.OrderType,
                PaymentProvider = placeOrderDto.PaymentMethod == "cash" ? "cash" : "paymob",
                OrderItems = [.. placeOrderDto.Items],
                DeliveryDetails = placeOrderDto.OrderType == OrderType.Delivery ? new DeliveryDetailsDto
                {
                    Address = placeOrderDto.DeliveryDetails?.Address ?? string.Empty,
                    Latitude = placeOrderDto.DeliveryDetails?.Latitude ?? 0,
                    Longitude = placeOrderDto.DeliveryDetails?.Longitude ?? 0,
                } : null,
            };

            var redirectUrl = string.Empty;

            switch (intent.PaymentProvider.ToLower())
            {
                case "cash":
                    var order = await ConfirmOrder(intent);
                    intent.OrderId = order!.OrderId;
                    await _paymentService.AddPayment(intent, "CASH_PAYMENT");
                    break;

                case "paymob":
                    var intentResponse = await CreatePaymentSession(intent)
                        ?? throw new InvalidOperationException("Failed to create payment session with Paymob.");

                    if (string.IsNullOrEmpty(intentResponse.RedirectUrl)) throw new InvalidOperationException("Redirect URL is empty from Paymob response.");

                    redirectUrl = intentResponse.RedirectUrl;
                    intent.Id = intentResponse.OrderId;
                    break;
                default:
                    throw new NotSupportedException($"Payment provider {intent.PaymentProvider} is not supported.");
            }


            var savedIntent = await _orderRepo.CreateOrderIntent(intent);

            return new OrderIntentDto
            {
                OrderIntentId = savedIntent.Id,
                RedirectUrl = redirectUrl,
                OrderId = intent.OrderId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order intent: {ex.Message}");
            throw;
        }
    }

    public async Task<IntentResponse> CreatePaymentSession(OrderIntent orderIntent)
    {
        return await _paymobService.CreateOrderIntent(orderIntent.Amount, orderIntent.PaymentMethod);
    }

    public async Task<OrderDto> ProceedTransaction(string orderIntentId, string transactionId)
    {
        var orderIntent = await _orderRepo.GetOrderIntentById(orderIntentId) ?? throw new KeyNotFoundException($"Order intent with id {orderIntentId} not found.");
        if (orderIntent.Status != PaymentStatus.INITIATED) throw new InvalidOperationException($"Order intent with id {orderIntentId} is not in a valid state for processing.");

        var order = await ConfirmOrder(orderIntent, true) ?? throw new InvalidOperationException($"Failed to confirm order for order intent with id {orderIntentId}.");

        var payment = await ProcessPayment(orderIntent, transactionId) ?? throw new InvalidOperationException($"Failed to process payment for order intent with id {orderIntentId}.");
        orderIntent.Status = PaymentStatus.Completed;

        return order;
    }

    private async Task<OrderDto?> ConfirmOrder(OrderIntent orderIntent, bool isPaid = false)
    {
        var existingOrder = await _orderRepo.GetOrderById(orderIntent.OrderId!);
        if (existingOrder != null)
        {
            Console.WriteLine($"Order with ID {orderIntent.OrderId} already exists. Skipping order creation.");
            return null;
        }

        var order = new Order
        {
            BranchId = orderIntent.BranchId,
            CustomerId = orderIntent.CustomerId,
            OrderType = orderIntent.OrderType,
            TotalAmount = orderIntent.Amount,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            IsPaid = isPaid,
            CustomerName = orderIntent.CustomerName,
            CustomerPhone = orderIntent.CustomerPhone,
        };

        order = await _orderRepo.CreateOrder(order);

        var orderDto = new OrderDto
        {
            OrderId = order.Id,
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = order.CustomerName ?? "Unknown Customer",
            CustomerPhone = order.CustomerPhone ?? "Unknown Phone",
            OrderType = orderIntent.OrderType.ToString(),
            PaymentMethod = orderIntent.PaymentMethod,
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            CustomerId = order.CustomerId,
        };

        List<OrderItem> orderItems = [];

        if (orderIntent.OrderItems != null && orderIntent.OrderItems.Count != 0)
        {
            foreach (var item in orderIntent.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    ItemId = item.ItemId,
                    OrderId = order.Id,
                    Quantity = item.Quantity,
                    Price = item.Price,
                };

                var savedOrderItem = await _orderRepo.CreateOrderItem(orderItem);
                if (savedOrderItem == null)
                {
                    throw new InvalidOperationException("Failed to create order item.");
                }

                await _itemAvailabilityService.DecreaseItemQuantity(item.ItemId, order.BranchId, item.Quantity);
                orderItems.Add(savedOrderItem);
            }

            order.OrderItems = orderItems;
        }


        switch (orderIntent.OrderType)
        {
            case OrderType.Takeaway:

                var takeaway = await PlaceTakeawayOrder(order);
                orderDto.OrderNumber = takeaway.OrderNumber;
                OrderEvents.OnOrderPlaced(order.BranchId, orderItems);
                break;

            case OrderType.Delivery:
                var delivery = await PlaceDeliveryOrder(order, orderIntent);
                OrderEvents.OnOrderPlaced(order.BranchId, orderItems);
                break;

            default:
                throw new NotImplementedException($"Order type {orderIntent.OrderType} is not implemented yet.");
        }

        return orderDto;
    }

    private async Task<Takeaway> PlaceTakeawayOrder(Order order)
    {
        var orderNum = DailyNumberGenerator.GetNextNumber();

        var takeaway = new Takeaway
        {
            OrderId = order.Id,
            OrderNumber = orderNum,
        };

        return await _orderRepo.CreateTakeawayOrder(takeaway);
    }

    private async Task<Delivery> PlaceDeliveryOrder(Order order, OrderIntent orderIntent)
    {

        if (orderIntent.DeliveryDetails == null)
        {
            throw new ArgumentNullException("Delivery details are required for delivery orders.");
        }

        var delivery = new Delivery
        {
            OrderId = order.Id,
            Address = orderIntent.DeliveryDetails.Address,
            Latitude = orderIntent.DeliveryDetails.Latitude,
            Longitude = orderIntent.DeliveryDetails.Longitude,
        };

        return await _orderRepo.CreateDeliveryOrder(delivery);
    }

    private async Task<PaymentDto> ProcessPayment(OrderIntent orderIntent, string transactionId)
    {
        return await _paymentService.AddPayment(orderIntent, transactionId);
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrders(string customerId)
    {
        var takeaways = await _orderRepo.GetTakeawaysByCustomerId(customerId);

        if (takeaways == null)
        {
            Console.WriteLine($"No takeaways orders found for customer with ID: {customerId}");
            takeaways = [];
        }

        var takeawayDtos = takeaways.Select(t => new OrderDto
        {
            OrderId = t.Order!.Id,
            PharmacyName = t.Order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = t.Order.Customer?.Username ?? "Unknown Customer",
            OrderType = OrderType.Takeaway.ToString(),
            PaymentMethod = t.Order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = t.Order.OrderDate,
            OrderStatus = t.Order.Status.ToString(),
            TotalAmount = t.Order.TotalAmount,
            BranchId = t.Order.BranchId,
            OrderNumber = t.OrderNumber,
            IsPaid = t.Order.IsPaid,
            CustomerId = t.Order.CustomerId,
            CustomerPhone = t.Order.CustomerPhone ?? "Unknown Phone"
        });

        var orders = takeawayDtos.OrderByDescending(o => o.OrderDate);
        return orders;

    }

    public async Task<OrderDto> GetOrderById(string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId);

        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found.");

        return new OrderDto
        {
            OrderId = order.Id,
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = order.CustomerName ?? "Unknown Customer",
            OrderType = "",
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            CustomerId = order.CustomerId,
            CustomerPhone = order.CustomerPhone ?? "Unknown Phone"
        };
    }

    public async Task<OrderDto> GetOrderDetails(string orderId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId);
        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found.");

        var orderDto = new OrderDto
        {
            OrderId = order.Id,
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = order.CustomerName ?? "Unknown Customer",
            CustomerId = order.Customer?.Id ?? string.Empty,
            CustomerPhone = order.CustomerPhone ?? "Unknown Phone",
            OrderType = order.OrderType.ToString(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            OrderNumber = order.OrderType == OrderType.Takeaway ? order.Takeaway?.OrderNumber : null,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Item = new ItemDto
                {
                    Id = oi.Item?.Id ?? string.Empty,
                    Name = oi.Item?.Name ?? "Unknown Item",
                    Description = oi.Item?.Description ?? "No description available",
                    ImageUrl = oi.Item?.ImageUrl ?? string.Empty,
                    Price = oi.Item?.Price ?? 0,
                    Category = oi.Item?.Category ?? "Uncategorized",
                    Priority = oi.Item?.Priority ?? 0,
                    Tags = oi.Item?.Tags ?? string.Empty,
                    Brand = oi.Item?.Brand ?? "Unknown Brand",
                },
                Quantity = oi.Quantity,
                Price = oi.Price,
            }).ToArray()

        };

        return orderDto;

    }

    public async Task<bool> ManualPayOrder(string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId);
        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found.");

        if (order.IsPaid) return true;

        order.IsPaid = true;
        order = await _orderRepo.SetOrderPaidStatus(orderId, true);

        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found after updating payment status.");

        return order.IsPaid;
    }

    public async Task<IEnumerable<OrderDto>> GetReadyOrders(string branchId)
    {
        var orders = await _orderRepo.GetReadyOrdersByBranchId(branchId);

        if (orders == null || !orders.Any())
        {
            Console.WriteLine($"No ready orders found for branch with ID: {branchId}");
            return [];
        }

        return orders.Select(o => new OrderDto
        {
            OrderId = o.Id,
            PharmacyName = o.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = o.CustomerName ?? "Unknown Customer",
            CustomerPhone = o.CustomerPhone ?? "Unknown Phone",
            OrderType = o.OrderType.ToString(),
            PaymentMethod = o.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = o.OrderDate,
            OrderStatus = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            BranchId = o.BranchId,
            IsPaid = o.IsPaid,
            CustomerId = o.CustomerId,
        });
    }

    public async Task<IEnumerable<OrderDto>> GetUnpaidOrders(string branchId)
    {
        var orders = await _orderRepo.GetUnpaidOrdersByBranchId(branchId);

        if (orders == null || !orders.Any())
        {
            Console.WriteLine($"No ready orders found for branch with ID: {branchId}");
            return [];
        }

        return orders.Select(o => new OrderDto
        {
            OrderId = o.Id,
            PharmacyName = o.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = o.CustomerName ?? "Unknown Customer",
            CustomerPhone = o.CustomerPhone ?? "Unknown Phone",
            OrderType = o.OrderType.ToString(),
            PaymentMethod = o.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = o.OrderDate,
            OrderStatus = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            BranchId = o.BranchId,
            IsPaid = o.IsPaid,
            CustomerId = o.CustomerId,
        });
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByBranch(string branchId)
    {
        var orders = await _orderRepo.GetAllOrdersByBranchId(branchId);

        if (orders == null || !orders.Any())
        {
            Console.WriteLine($"No orders found for branch with ID: {branchId}");
            return [];
        }

        return orders.Select(OrdersDtoMapper.ToDto);
    }

    public async Task<OrderInvoiceDto?> PickOrder(string orderId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId) ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");

        if (order.Status > OrderStatus.Ready)
        {
            Console.WriteLine($"Order with id {orderId} is already picked or delivered.");
            throw new InvalidOperationException($"Order with id {orderId} is not in a valid state for picking.");
        }

        if (order.IsPaid == false)
        {
            Console.WriteLine($"Order with id {orderId} is not paid yet.");
            return null!;
        }

        await _orderRepo.SetOrderStatus(orderId, OrderStatus.Delivered);

        string orderNumber = "";

        switch (order.OrderType)
        {
            case OrderType.Takeaway:
                var takeaway = await _orderRepo.GetTakeawayById(orderId);
                if (takeaway != null)
                {
                    orderNumber = takeaway.OrderNumber.ToString();
                }
                break;

            default:
                throw new NotImplementedException($"Order type {order.OrderType} is not implemented yet.");
        }

        return new OrderInvoiceDto
        {
            OrderId = order.Id,
            OrderNumber = orderNumber ?? "N/A",
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            CustomerName = order.Customer?.Username ?? "Unknown Customer",
            OrderType = order.OrderType.ToString(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            BranchAddress = order.Branch?.Address ?? "Unknown Address",
            IsPaid = order.IsPaid,
            Items = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Item = new ItemDto
                {
                    Id = oi.Item?.Id ?? string.Empty,
                    Name = oi.Item?.Name ?? "Unknown Item",
                    Description = oi.Item?.Description ?? "No description available",
                    Price = oi.Item?.Price ?? 0,
                    Category = oi.Item?.Category ?? "Uncategorized",
                    ImageUrl = oi.Item?.ImageUrl ?? string.Empty,
                    Priority = oi.Item?.Priority ?? 0,
                    Tags = oi.Item?.Tags ?? string.Empty,
                    Brand = oi.Item?.Brand ?? "Unknown Brand",
                },
                Quantity = oi.Quantity,
                Price = oi.Price,
            }).ToList() ?? []
        };
    }

    public async Task<IEnumerable<OrderDto>> GetPaidOrders(string branchId)
    {
        var orders = await _orderRepo.GetPaidOrdersOfBranch(branchId);

        if (orders == null || !orders.Any())
        {
            Console.WriteLine($"No paid orders found for branch with ID: {branchId}");
            return [];
        }

        return orders.Select(o => new OrderDto
        {
            OrderId = o.Id,
            PharmacyName = o.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = o.CustomerName ?? "Unknown Customer",
            CustomerPhone = o.CustomerPhone ?? "Unknown Phone",
            OrderType = o.OrderType.ToString(),
            PaymentMethod = o.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = o.OrderDate,
            OrderStatus = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            BranchId = o.BranchId,
            IsPaid = o.IsPaid,
            CustomerId = o.CustomerId,
        });
    }

    public async Task<DeliverRequestDto> CreateDeliverRequest(string orderId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId) ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");
        if (order.Status != OrderStatus.Ready)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not in a valid state for delivery.");
        }

        var deliverRequest = new DeliverRequest
        {
            OrderId = orderId,
            Status = DeliverStatus.Pending,
        };

        var createdRequest = await _deliverRequestRepo.AddDeliverRequest(deliverRequest);

        var jobId = _backgroundJobs.Schedule(() => ConfirmDeliverRequest(createdRequest.OrderId), TimeSpan.FromMinutes(15));

        if (string.IsNullOrEmpty(jobId))
        {
            throw new InvalidOperationException("Failed to schedule background job for delivery confirmation.");
        }

        _jobIds.Add(createdRequest.OrderId, jobId);

        var token = await _cloudMessaging.GetTokenByUserId(order.CustomerId);
        await _cloudMessaging.SendNotificationAsync(token, "Did you receive your order?", $"Your order from {order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy"}. Will considered as a yes after 15 minutes.", new Dictionary<string, string>
        {
            { "type", "DELIVER_CONFIRMATION" },
            { "orderId", createdRequest.OrderId },
        });

        return new DeliverRequestDto
        {
            OrderId = createdRequest.OrderId,
            Status = createdRequest.Status
        };
    }

    public async Task<bool> ConfirmDeliverRequest(string orderId)
    {
        var deliverRequest = await _deliverRequestRepo.GetDeliverRequestById(orderId)
        ?? throw new KeyNotFoundException($"Deliver request with order id {orderId} not found.");

        if (deliverRequest.Status != DeliverStatus.Pending)
        {
            throw new InvalidOperationException($"Deliver request with order id {orderId} is not in a valid state for confirmation.");
        }

        deliverRequest.Status = DeliverStatus.Confirmed;
        await _deliverRequestRepo.UpdateDeliverRequest(deliverRequest);
        await _orderRepo.SetOrderStatus(orderId, OrderStatus.Delivered);
        if (_jobIds.TryGetValue(orderId, out var jobId))
        {
            BackgroundJob.Delete(jobId);
            _jobIds.Remove(orderId);
        }

        return true;
    }

    public async Task<bool> CancelDeliverRequest(string orderId)
    {
        var deliverRequest = await _deliverRequestRepo.GetDeliverRequestById(orderId)
        ?? throw new KeyNotFoundException($"Deliver request with order id {orderId} not found.");

        if (deliverRequest.Status != DeliverStatus.Pending)
        {
            throw new InvalidOperationException($"Deliver request with order id {orderId} is not in a valid state for decline.");
        }

        deliverRequest.Status = DeliverStatus.Cancelled;
        await _deliverRequestRepo.UpdateDeliverRequest(deliverRequest);
        await _orderRepo.SetOrderStatus(orderId, OrderStatus.Cancelled);

        if (_jobIds.TryGetValue(orderId, out var jobId))
        {
            BackgroundJob.Delete(jobId);
            _jobIds.Remove(orderId);
        }

        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetTakeawayOrders(string branchId)
    {
        var orders = await _orderRepo.GetAllTakeawaysByBranchId(branchId);
        return orders.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            Customer = o.Order!.CustomerName!,
            CustomerId = o.Order.Customer?.Id ?? string.Empty,
            CustomerPhone = o.Order.CustomerPhone ?? "Unknown Phone",
            TotalAmount = o.Order.TotalAmount,
            OrderDate = o.Order.OrderDate,
            OrderStatus = o.Order.Status.ToString(),
            BranchId = o.Order.BranchId,
            IsPaid = o.Order.IsPaid,
            OrderType = o.Order.OrderType.ToString(),
            PaymentMethod = o.Order.Payment?.PaymentMethod ?? "Unknown",
            PharmacyName = o.Order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            OrderNumber = o.OrderNumber
        });
    }

    public async Task<bool> MarkOrderAsPaid(string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId) ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");

        if (order.Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not in a valid state for payment.");
        }

        await _orderRepo.SetOrderPaidStatus(orderId, true);

        return true;
    }

    public async Task<bool> CancelOrderAsync(string orderId)
    {
        var order = await _orderRepo.SetOrderStatus(orderId, OrderStatus.Cancelled);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {orderId} not found.");
        }

        OrderEvents.OnOrderCancelled(order.BranchId, orderId);

        return order != null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersQueueByBranchId(string branchId)
    {
        try
        {
            var orders = await _orderRepo.GetOrdersQueueByBranchId(branchId);

            if (orders == null || !orders.Any())
            {
                Console.WriteLine($"No orders in queue found for branch with ID: {branchId}");
                return [];
            }

            return OrdersDtoMapper.ToDtoList(orders);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching orders queue: {ex.Message}");
            throw;
        }
    }

    public async Task<OrderDto> ConfirmPendingOrder(string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId) ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not in a valid state for confirmation.");
        }

        var newStatus = order.OrderType switch
        {
            OrderType.Takeaway => OrderStatus.Ready,
            OrderType.Delivery => OrderStatus.Queued,
            _ => throw new NotImplementedException($"Order type {order.OrderType} is not implemented yet."),
        };

        order = await _orderRepo.SetOrderStatus(orderId, newStatus) ?? throw new InvalidOperationException($"Failed to confirm order with id {orderId}.");

        return new OrderDto
        {
            OrderId = order.Id,
            PharmacyName = order.Branch?.Pharmacy?.Name ?? "Unknown Pharmacy",
            Customer = order.CustomerName ?? "Unknown Customer",
            CustomerPhone = order.CustomerPhone ?? "Unknown Phone",
            OrderType = order.OrderType.ToString(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            CustomerId = order.CustomerId,
        };
    }

    public async Task<OrderDto> SetOrderAsDelivered(string orderId)
    {

        try
        {
            var order = await _orderRepo.GetOrderById(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {orderId} not found");
            }

            if (order.Status != OrderStatus.Ready && order.Status != OrderStatus.Queued)
            {
                throw new InvalidOperationException($"Order status is invalid");
            }

            var updatedOrder = await _orderRepo.SetOrderStatus(order.Id, OrderStatus.Delivered);
            return OrdersDtoMapper.ToDto(updatedOrder!);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersOutForDelivery(string branchId)
    {
        var orders = await _orderRepo.GetOrdersOutForDelivery(branchId);

        if (orders == null || !orders.Any())
        {
            Console.WriteLine($"No delivery orders found for branch with ID: {branchId}");
            return [];
        }

        return orders.Select(OrdersDtoMapper.ToDto);
    }

}

public class IntentResponse
{
    public required string OrderId { get; set; }
    public required string RedirectUrl { get; set; }
}
