using System.Text;
using System.Text.Json;
using OrdrMate.Managers;
using OrdrMate.Models;
using OrdrMate.Repositories;

namespace OrdrMate.Services;

public class AiService
{
    private static string _aiBaseUrl = "http://localhost:8000/api/ai";
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _env;
    private readonly OrderManager _orderManager;
    private readonly IOrderRepo _orderRepo;


    public AiService(
        HttpClient httpClient,
        IWebHostEnvironment env,
        IConfiguration configuration,
        OrderManager orderManager,
        IOrderRepo orderRepo
    )
    {
        _httpClient = httpClient;
        _orderManager = orderManager;
        _orderRepo = orderRepo;
        _env = env;

        if (_env.IsDevelopment())
        {
            _aiBaseUrl = "http://localhost:8000";
        }
        else if (_env.IsProduction())
        {
            _aiBaseUrl = configuration["Ai:BaseUrl"] ?? "https://api.ordrmate.com/ai";
        }
    }

    public async Task<decimal> PredictStayDuration(TableReservation reservation)
    {
        if (reservation == null || string.IsNullOrEmpty(reservation.BranchId))
        {
            throw new ArgumentException("Invalid reservation data.");
        }

        var requestUrl = $"{_aiBaseUrl}/predict";

        Console.WriteLine($"Predicting stay duration for reservation: {reservation.ReservationId} at branch {reservation.BranchId}");

        var timeToPrepareOrder = await _orderManager.GetEstimatedTimeForOrder(reservation.BranchId, reservation.OrderId);

        List<ItemRequest> items = [];

        if (reservation.Order?.OrderItems != null)
        {
            items = [.. reservation.Order.OrderItems.Select(i => new ItemRequest
            {
                item_name = i.Item?.Name!,
                quantity = i.Quantity
            })];
        }
        else
        {
            var order = await _orderRepo.GetDetailedOrderById(reservation.OrderId);
            if (order?.OrderItems != null)
            {
                items = [.. order.OrderItems.Select(i => new ItemRequest
                {
                    item_name = i.Item?.Name!,
                    quantity = i.Quantity
                })];
            }
        }

        var body = new
        {
            samples = new[]
            {
                new {
                    customer_id = reservation.CustomerId,
                    branch_id = reservation.BranchId,
                    date = reservation.ReservationTime.Date.ToString("yyyy-MM-dd"),
                    time_hour = reservation.ReservationTime.Hour,
                    wait_time = timeToPrepareOrder,
                    items
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(requestUrl, body);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to predict stay duration: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<AiPredictionResult>();
        if (result == null || result.predictions == null || result.predictions.Count == 0)
        {
            throw new Exception("Invalid prediction result received from AI service.");
        }
        return result.predictions[0];
    }
}

class AiPredictionResult
{
    public required List<decimal> predictions { get; set; }
}

class ItemRequest
{
    public required string item_name { get; set; }
    public required int quantity { get; set; }
}