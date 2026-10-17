using PresentationMVC.Models;

namespace PresentationMVC.Services
{
    public sealed class StaffApiClient
    {
        private const string Resource = "api/admin/staffs";
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffApiClient> _logger;

        public StaffApiClient(HttpClient httpClient, ILogger<StaffApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<StaffListResponse> GetStaffsListAsync(StaffListRequest request, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync($"{Resource}/list", request, cancellationToken);
            return await ReadAsync<StaffListResponse>(response, cancellationToken) ?? new();
        }

        public async Task<StaffModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadAsync<StaffModel>(response, cancellationToken);
        }

        public async Task CreateAsync(StaffCreateModel model, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(Resource, model, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdateAsync(int id, StaffUpdateModel model, CancellationToken cancellationToken = default)
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
            _logger.LogWarning("Staff API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Staff API request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }
    }

    public class ToggleStatusResult
    {
        public bool Success { get; set; }
        public bool IsActive { get; set; }
    }
}
