namespace OrdrMate.Services;

public class PaymobService(HttpClient httpClient, IConfiguration configuration)
{
    private const string PaymobBaseUrl = "https://accept.paymob.com/api";
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["Paymob:ApiKey"]
    ?? throw new ArgumentNullException("Paymob API Key is not configured.");
    private readonly int _integrationIdCard = int.Parse(configuration["Paymob:IntegrationIdCard"]!);
    private readonly int iFrameId = int.Parse(configuration["Paymob:IFrameId"]!);

    public async Task<IntentResponse> CreateOrderIntent(decimal amount, string paymentMethod)
    {
        var authToken = await GetAuthToken();
        var orderId = await CreatePaymentOrder(authToken, amount);
        var paymentKey = await GetPaymentKey(authToken, orderId, amount, _integrationIdCard);

        return new IntentResponse
        {
            OrderId = orderId.ToString(),
            RedirectUrl = $"{PaymobBaseUrl}/acceptance/iframes/{iFrameId}?payment_token={paymentKey}"
        };
    }

    private async Task<string> GetAuthToken()
    {
        var response = await _httpClient.PostAsJsonAsync($"{PaymobBaseUrl}/auth/tokens", new
        {
            api_key = _apiKey
        });

        var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
        return result?.Token ?? throw new Exception("Failed to retrieve Paymob auth token.");
    }

    private async Task<int> CreatePaymentOrder(string authToken, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync($"{PaymobBaseUrl}/ecommerce/orders", new
        {
            auth_token = authToken,
            delivery_needed = false,
            amount_cents = amount,
            currency = "EGP",
            items = Array.Empty<object>()
        });

        var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>();
        return result?.Id ?? throw new Exception("Failed to create Paymob payment order.");
    }

    private async Task<string> GetPaymentKey(string authToken, int orderId, decimal amount, int integrationId)
    {
        var response = await _httpClient.PostAsJsonAsync($"{PaymobBaseUrl}/acceptance/payment_keys", new
        {
            auth_token = authToken,
            amount_cents = amount * 100,
            expiration = 3600,
            order_id = orderId,
            integration_id = integrationId,
            currency = "EGP",
            billing_data = new
            {
                apartment = "NA",
                email = "example@gmail.com",
                floor = "NA",
                first_name = "John",
                street = "NA",
                building = "NA",
                phone_number = "01000000000",
                shipping_method = "NA",
                postal_code = "NA",
                city = "Cairo",
                country = "EG",
                last_name = "Doe",
                state = "NA"
            }
        });

        var result = await response.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>();
        return result?.Token ?? throw new Exception("Failed to retrieve Paymob payment key.");
    }

    private class PaymobAuthResponse { public required string Token { get; set; } }
    private class PaymobOrderResponse { public int Id { get; set; } }
    private class PaymobPaymentKeyResponse { public required string Token { get; set; } }
    

}