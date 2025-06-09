using OrdrMate.Enums;
using OrdrMate.DTOs.Order;
using OrdrMate.Models;
using OrdrMate.Utils;
using OrdrMate.Repositories;
using OrdrMate.Events;

namespace OrdrMate.Services;

public class OrderService
{
    private readonly PaymentService _paymentService;
    private readonly IOrderRepo _orderRepo;
    private readonly PaymobService _paymobService;
    private readonly TableService _tableService;

    public OrderService(PaymentService paymentService, IOrderRepo orderRepo, PaymobService paymobService, TableService tableService)
    {
        _paymentService = paymentService;
        _orderRepo = orderRepo;
        _paymobService = paymobService;
        _tableService = tableService;
    }

    public async Task<OrderIntentDto> CreateOrderIntent(PlaceOrderDto placeOrderDto)
    {

        var totalAmount = placeOrderDto.Items.Sum(oi => oi.Price * oi.Quantity);

        var intent = new OrderIntent
        {
            CustomerId = placeOrderDto.CustomerId,
            BranchId = placeOrderDto.BranchId,
            Status = PaymentStatus.INITIATED,
            Amount = totalAmount,
            PaymentMethod = placeOrderDto.PaymentMethod,
            OrderType = placeOrderDto.OrderType,
            PaymentProvider = placeOrderDto.PaymentMethod == "cash" ? "cash" : "paymob",
            OrderItems = [.. placeOrderDto.Items.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Quantity = oi.Quantity,
                Price = oi.Price,
            })],
            TableNumber = placeOrderDto.TableNumber,
        };

        var redirectUrl = string.Empty;

        switch (intent.PaymentProvider.ToLower())
        {
            case "cash":
                var order = await ConfirmOrder(intent);
                intent.OrderId = order!.OrderId;
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
        };
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
            Status = OrderStatus.Queued,
            IsPaid = isPaid,
        };

        order = await _orderRepo.CreateOrder(order);

        var orderDto = new OrderDto
        {
            OrderId = order.Id,
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = order.Customer?.Username ?? "Unknown Customer",
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

                orderItem = await _orderRepo.CreateOrderItem(orderItem);
                orderItems.Add(orderItem);
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

            case OrderType.DineIn:
                var reservation = await _tableService.ReserveTable(orderDto, orderIntent.TableNumber ?? 1);
                orderDto.TableNumber = reservation.TableNumber;
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

    private async Task<PaymentDto> ProcessPayment(OrderIntent orderIntent, string transactionId)
    {
        return await _paymentService.AddPayment(orderIntent, transactionId);
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrders(string customerId)
    {
        var takeaways = await _orderRepo.GetTakeawaysByCustomerId(customerId);
        var indoors = await _tableService.GetCustomerTableReservation(customerId);

        if (takeaways == null)
        {
            Console.WriteLine($"No takeaways orders found for customer with ID: {customerId}");
            takeaways = [];
        }

        if (indoors == null)
        {
            Console.WriteLine($"No dine-in orders found for customer with ID: {customerId}");
            indoors = [];
        }

        var takeawayDtos = takeaways.Select(t => new OrderDto
        {
            OrderId = t.Order.Id,
            RestaurantName = t.Order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = t.Order.Customer?.Username ?? "Unknown Customer",
            OrderType = OrderType.Takeaway.ToString(),
            PaymentMethod = t.Order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = t.Order.OrderDate,
            OrderStatus = t.Order.Status.ToString(),
            TotalAmount = t.Order.TotalAmount,
            BranchId = t.Order.BranchId,
            OrderNumber = t.OrderNumber,
            IsPaid = t.Order.IsPaid,
            CustomerId = t.Order.CustomerId
        });

        var indoorDtos = indoors.Select(i => new OrderDto
        {
            OrderId = i.Order!.Id,
            RestaurantName = i.Order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = i.Order.Customer?.Username ?? "Unknown Customer",
            OrderType = OrderType.DineIn.ToString(),
            PaymentMethod = i.Order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = i.Order.OrderDate,
            OrderStatus = i.Order.Status.ToString(),
            TotalAmount = i.Order.TotalAmount,
            BranchId = i.Order.BranchId,
            TableNumber = i.TableNumber,
            IsPaid = i.Order.IsPaid,
            CustomerId = i.Order.CustomerId
        });

        var orders = takeawayDtos.Concat(indoorDtos);

        return orders;

    }

    public async Task<OrderDto> GetOrderById(string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId);

        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found.");

        return new OrderDto
        {
            OrderId = order.Id,
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = order.Customer?.Username ?? "Unknown Customer",
            OrderType = "",
            PaymentMethod = order.Payment?.PaymentMethod ?? "Unknown",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            CustomerId = order.CustomerId,
        };
    }

    public async Task<OrderDto> GetOrderDetails(string orderId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId);
        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found.");

        var orderDto = new OrderDto
        {
            OrderId = order.Id,
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = order.Customer?.Username ?? "Unknown Customer",
            CustomerId = order.Customer?.Id ?? string.Empty,
            OrderType = order.OrderType.ToString(),
            PaymentMethod = order.Payment?.PaymentMethod ?? "Cash",
            OrderDate = order.OrderDate,
            OrderStatus = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            BranchId = order.BranchId,
            IsPaid = order.IsPaid,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                ItemId = oi.ItemId,
                Item = new DTOs.Item.ItemDto
                {
                    Id = oi.Item?.Id ?? string.Empty,
                    Name = oi.Item?.Name ?? "Unknown Item",
                    Description = oi.Item?.Description ?? "No description available",
                    ImageUrl = oi.Item?.ImageUrl ?? string.Empty,
                    Price = oi.Item?.Price ?? 0,
                    Category = oi.Item?.CategoryName ?? "Uncategorized",
                    PreparationTime = oi.Item?.PreperationTime ?? 0,
                    KitchenName = oi.Item?.Kitchen?.Name ?? "Unknown Kitchen"
                },
                Quantity = oi.Quantity,
                Price = oi.Price,
            }).ToArray()

        };

        var takeaway = await _orderRepo.GetTakeawayById(orderId);

        if (takeaway != null)
        {
            orderDto.OrderNumber = takeaway.OrderNumber;
            return orderDto;
        }

        var indoor = await _tableService.GetTableReservationByOrderId(orderId);
        if (indoor != null)
        {
            orderDto.TableNumber = indoor.TableNumber;
            return orderDto;
        }

        throw new KeyNotFoundException($"Order with id {orderId} not found.");

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
            RestaurantName = o.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = o.Customer?.Username ?? "Unknown Customer",
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
            RestaurantName = o.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = o.Customer?.Username ?? "Unknown Customer",
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

        return orders.Select(o => new OrderDto
        {
            OrderId = o.Id,
            RestaurantName = o.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = o.Customer?.Username ?? "Unknown Customer",
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

    public async Task<OrderInvoiceDto> PickOrder(string orderId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId) ?? throw new KeyNotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Queued)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not in a valid state for picking.");
        }

        if (order.IsPaid == false)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not paid yet.");
        }

        if (order.Status != OrderStatus.Ready)
        {
            throw new InvalidOperationException($"Order with id {orderId} is not ready for pickup.");
        }

        order = await _orderRepo.SetOrderStatus(orderId, OrderStatus.Delivered);

        if (order == null) throw new KeyNotFoundException($"Order with id {orderId} not found after updating status.");

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

            case OrderType.DineIn:
                var indoor = await _tableService.GetTableReservationByOrderId(orderId);
                if (indoor != null)
                {
                    orderNumber = $"Table {indoor.TableNumber}" + $" ({indoor.ReservationTime})";
                }
                break;

            default:
                throw new NotImplementedException($"Order type {order.OrderType} is not implemented yet.");
        }

        return new OrderInvoiceDto
        {
            OrderId = order.Id,
            OrderNumber = orderNumber ?? "N/A",
            RestaurantName = order.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
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
                Item = new DTOs.Item.ItemDto
                {
                    Id = oi.Item?.Id ?? string.Empty,
                    Name = oi.Item?.Name ?? "Unknown Item",
                    Description = oi.Item?.Description ?? "No description available",
                    Price = oi.Item?.Price ?? 0,
                    Category = oi.Item?.CategoryName ?? "Uncategorized",
                    PreparationTime = oi.Item?.PreperationTime ?? 0,
                    KitchenName = oi.Item?.Kitchen?.Name ?? "Unknown Kitchen"
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
            RestaurantName = o.Branch?.Restaurant?.Name ?? "Unknown Restaurant",
            Customer = o.Customer?.Username ?? "Unknown Customer",
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
    
}

public class IntentResponse
{
    public required string OrderId { get; set; }
    public required string RedirectUrl { get; set; }
}