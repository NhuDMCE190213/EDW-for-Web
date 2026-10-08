using PresentationMVC.Models.Order.Customer;
using System.Text.Json;
using System.Text;

namespace PresentationMVC.Services
{
    public sealed class CustomerOrderApiClient
    {
        private const string Resource = "api/customer/orders";
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerOrderApiClient> _logger;

        public CustomerOrderApiClient(HttpClient httpClient, ILogger<CustomerOrderApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(int customerId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{customerId}", cancellationToken);
            return await ReadAsync<List<OrderDto>>(response, cancellationToken) ?? new();
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsync($"{Resource}/{orderId}/cancel", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<TransferResult?> TransferToCartAsync(Guid orderId, int customerId, CancellationToken cancellationToken = default)
        {
            var content = new StringContent(JsonSerializer.Serialize(customerId), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync($"{Resource}/{orderId}/transfer", content, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await ReadAsync<TransferResult>(response, cancellationToken);
        }

        private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Customer Order API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Customer Order API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }
    }

    public class TransferResult
    {
        public bool Success { get; set; }
        public int Transferred { get; set; }
        public int Total { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
