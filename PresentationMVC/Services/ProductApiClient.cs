using PresentationMVC.Models.Product.Staff;

namespace PresentationMVC.Services
{
    public sealed class ProductApiClient
    {
        private const string Resource = "api/staff/products";
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductStaffModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(Resource, cancellationToken);
            return await ReadAsync<List<ProductStaffModel>>(response, cancellationToken) ?? new();
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
