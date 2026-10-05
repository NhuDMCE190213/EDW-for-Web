using System.Net;
using System.Net.Http.Json;
using PresentationMVC.Models;

namespace PresentationMVC.Services;

public sealed class ProductVariantApiClient
{
    private const string Resource = "api/staff/product-variants";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductVariantApiClient> _logger;

    public ProductVariantApiClient(HttpClient httpClient, ILogger<ProductVariantApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ProductVariantModel>> GetByProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{Resource}/by-product/{productId}", cancellationToken);
        return await ReadAsync<List<ProductVariantModel>>(response, cancellationToken) ?? new();
    }

    public async Task<ProductVariantModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{Resource}/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadAsync<ProductVariantModel>(response, cancellationToken);
    }

    public async Task CreateAsync(ProductVariantCreateModel model, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(Resource, model, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(ProductVariantUpdateModel model, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{Resource}/{model.ProductVariantId}", model, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{Resource}/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task StockInAsync(Guid id, int amount, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{Resource}/{id}/stock-in", amount, cancellationToken);
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
        _logger.LogWarning("Product variant API returned {StatusCode}: {Body}", response.StatusCode, body);
        throw new HttpRequestException($"Product variant API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }
}
