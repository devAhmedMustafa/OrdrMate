using System.Text.Json;
using OrdrMate.Repositories;

namespace OrdrMate.Sockets;

public class CustomerOrdersSocketHandler : BaseSocketHandler
{
    private readonly IOrderRepo _orderRepo;
    public CustomerOrdersSocketHandler(IOrderRepo orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task NotifyOrderReady(string orderId, string customerId)
    {
        var orderReadyMessage = JsonSerializer.Serialize(new
        {
            eventType = "orderReady",
            orderId
        });

        await SendTo(customerId, orderReadyMessage);
    }

    public async Task AskForLocation(string orderId, string customerId)
    {
        var order = await _orderRepo.GetDetailedOrderById(orderId);
        if (order == null) return;

        var askForLocationMessage = JsonSerializer.Serialize(new
        {
            eventType = "askForLocation",
            orderId,
            branchId = order.BranchId,
        });

        await SendTo(customerId, askForLocationMessage);
    }

}