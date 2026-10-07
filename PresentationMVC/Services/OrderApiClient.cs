using PresentationMVC.Models.Order.Staff;
using System.Text.Json;
using System.Text;

namespace PresentationMVC.Services
{
    public sealed class OrderApiClient
    {
        private const string Resource = "api/staff/orders";
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderApiClient> _logger;

        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<OrderStaffModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(Resource, cancellationToken);
            return await ReadAsync<List<OrderStaffModel>>(response, cancellationToken) ?? new();
        }

        public async Task<OrderDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/dashboard", cancellationToken);
            return await ReadAsync<OrderDashboardModel>(response, cancellationToken) ?? new OrderDashboardModel();
        }

        public async Task<OrderStaffModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadAsync<OrderStaffModel>(response, cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
        {
            var content = new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync($"{Resource}/{id}/status", content, cancellationToken);
            return response.IsSuccessStatusCode;
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
            _logger.LogWarning("Order API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Order API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }
    }
}
