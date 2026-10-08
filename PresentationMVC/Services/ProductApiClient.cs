using PresentationMVC.Models;

namespace PresentationMVC.Services
{
    public sealed class ProductApiClient
    {
        private const string ResourceAdmin = "api/admin/products";
        private const string ResourceStaff = "api/staff/products";
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductStaffModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(ResourceStaff, cancellationToken);
            return await ReadAsync<List<ProductStaffModel>>(response, cancellationToken) ?? new();
        }

        public async Task<ProductStaffModel?> GetByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{ResourceAdmin}/{productId}", cancellationToken);
            return await ReadAsync<ProductStaffModel>(response, cancellationToken);
        }

        public async Task CreateAsync(ProductStaffCreateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(ResourceAdmin, model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateAsync(ProductStaffUpdateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync($"{ResourceAdmin}/{model.ProductId}", model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeleteAsync(int productId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"{ResourceAdmin}/{productId}", cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Product API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Product API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }
    }
}
