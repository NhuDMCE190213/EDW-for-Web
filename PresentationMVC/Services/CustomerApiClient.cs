using PresentationMVC.Models;

namespace PresentationMVC.Services
{
    public sealed class CustomerApiClient
    {
        private const string Resource = "api/admin/customers";
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerApiClient> _logger;

        public CustomerApiClient(HttpClient httpClient, ILogger<CustomerApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CustomerListResponse> GetCustomersListAsync(CustomerListRequest request, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync($"{Resource}/list", request, cancellationToken);
            return await ReadAsync<CustomerListResponse>(response, cancellationToken) ?? new();
        }

        public async Task<CustomerModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadAsync<CustomerModel>(response, cancellationToken);
        }

        public async Task CreateAsync(CustomerCreateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(Resource, model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateAsync(int id, CustomerUpdateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync($"{Resource}/{id}", model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsync($"{Resource}/{id}/toggle-status", null, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ToggleStatusResult>(cancellationToken: cancellationToken);
            return result?.IsActive ?? false;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"{Resource}/{id}", cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode) return;
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Customer API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Customer API request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }
    }
}
