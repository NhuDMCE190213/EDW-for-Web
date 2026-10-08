using PresentationMVC.Models;

namespace PresentationMVC.Services
{
    public sealed class CategoryApiClient
    {
        private const string Resource = "api/admin/categories";
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public CategoryApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CategoryModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(Resource, cancellationToken);
            return await ReadAsync<List<CategoryModel>>(response, cancellationToken) ?? new();
        }

        public async Task<CategoryModel?> GetByIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{productId}", cancellationToken);
            return await ReadAsync<CategoryModel>(response, cancellationToken);
        }

        public async Task CreateAsync(CategoryCreateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(Resource, model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateAsync(CategoryUpdateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync($"{Resource}/{model.Id}", model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"{Resource}/{categoryId}", cancellationToken);
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
