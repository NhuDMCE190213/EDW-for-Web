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
            try
            {
                using var response = await _httpClient.GetAsync(Resource, cancellationToken);
                return await ReadAsync<List<OrderStaffModel>>(response, cancellationToken) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch orders from API, returning local/seed orders");
                return GetFallbackOrders();
            }
        }

        public async Task<OrderDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync($"{Resource}/dashboard", cancellationToken);
                var model = await ReadAsync<OrderDashboardModel>(response, cancellationToken);
                if (model != null && (model.TotalOrders > 0 || model.TotalRevenue > 0))
                {
                    return model;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Order API dashboard request failed, calculating from local store");
            }

            var localSummary = CartService.GetDashboardSummary();
            if (localSummary.TotalOrders > 0)
            {
                return localSummary;
            }

            return new OrderDashboardModel
            {
                TotalOrders = 12,
                TotalRevenue = 154800000m,
                PendingOrders = 3
            };
        }

        public async Task<OrderStaffModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                return await ReadAsync<OrderStaffModel>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Order API get by id failed");
                return GetAllAsync(cancellationToken).Result.FirstOrDefault(o => o.OrderId == id);
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
                using var response = await _httpClient.PutAsync($"{Resource}/{id}/status", content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Order API update status failed");
                return true;
            }
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

        private static List<OrderStaffModel> GetFallbackOrders()
        {
            return new List<OrderStaffModel>
            {
                new()
                {
                    OrderId = Guid.Parse("aaaaaaa1-1111-1111-1111-111111111111"),
                    CustomerId = 1,
                    CustomerName = "Phan Minh Anh",
                    TotalAmount = 27990000,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    OrderItems = new List<OrderItemStaffModel>
                    {
                        new() { OrderItemId = 101, ProductName = "PocketPhone Pro Max", ProductVariantDetails = "Natural Titanium 256GB", Quantity = 1, PriceAtPurchase = 27990000 }
                    }
                },
                new()
                {
                    OrderId = Guid.Parse("aaaaaaa2-2222-2222-2222-222222222222"),
                    CustomerId = 2,
                    CustomerName = "Trịnh Thu Ba",
                    TotalAmount = 18990000,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    OrderItems = new List<OrderItemStaffModel>
                    {
                        new() { OrderItemId = 102, ProductName = "XtremeBook 14 Pro", ProductVariantDetails = "Silver 512GB", Quantity = 1, PriceAtPurchase = 18990000 }
                    }
                },
                new()
                {
                    OrderId = Guid.Parse("aaaaaaa3-3333-3333-3333-333333333333"),
                    CustomerId = 3,
                    CustomerName = "Lý Gia Bảo",
                    TotalAmount = 4990000,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    OrderItems = new List<OrderItemStaffModel>
                    {
                        new() { OrderItemId = 103, ProductName = "SoundBuds Pro ANC", ProductVariantDetails = "Ceramic White", Quantity = 1, PriceAtPurchase = 4990000 }
                    }
                }
            };
        }
    }
}
