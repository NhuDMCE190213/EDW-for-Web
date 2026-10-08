using PresentationMVC.Models.Promotion;

namespace PresentationMVC.Services
{
    public sealed class PromotionApiClient
    {
        private const string Resource = "api/promotions";
        private readonly HttpClient _httpClient;
        private readonly ILogger<PromotionApiClient> _logger;

        public PromotionApiClient(HttpClient httpClient, ILogger<PromotionApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PromotionListResponse> GetPromotionsListAsync(PromotionListRequest request, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync($"{Resource}/list", request, cancellationToken);
            return await ReadAsync<PromotionListResponse>(response, cancellationToken) ?? new();
        }

        public async Task<PromotionDto?> GetPromotionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadAsync<PromotionDto>(response, cancellationToken);
        }

        public async Task<List<PromotionDto>> GetActiveAndUpcomingPromotionsAsync(CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"{Resource}/active", cancellationToken);
            return await ReadAsync<List<PromotionDto>>(response, cancellationToken) ?? new();
        }

        public async Task CreatePromotionAsync(PromotionCreateModel dto, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(Resource, dto, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task UpdatePromotionAsync(Guid id, PromotionUpdateModel dto, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync($"{Resource}/{id}", dto, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeletePromotionAsync(Guid id, CancellationToken cancellationToken = default)
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
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Promotion API returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Promotion API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }
    }
}
