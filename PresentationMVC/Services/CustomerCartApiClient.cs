using System.Net.Http.Json;
using PresentationMVC.Models;
using PresentationMVC.Models.Cart;

namespace PresentationMVC.Services;

public sealed class CustomerCartApiClient
{
    private const string Resource = "api/customer/cart";
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerCartApiClient> _logger;

    public CustomerCartApiClient(HttpClient httpClient, ILogger<CustomerCartApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task AddAsync(CartItemCreateModel model, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(Resource, model, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<List<CartItemDto>> GetAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{Resource}/{customerId}", cancellationToken);
        return await ReadAsync<List<CartItemDto>>(response, cancellationToken) ?? new List<CartItemDto>();
    }

    public async Task<CartItemDto?> UpdateAsync(CartItemUpdateDto model, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{Resource}/{model.CartItemId}", model, cancellationToken);
        return await ReadAsync<CartItemDto>(response, cancellationToken);
    }

    public async Task DeleteAsync(int cartItemId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{Resource}/{cartItemId}", cancellationToken);
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
        _logger.LogWarning("Customer cart API returned {StatusCode}: {Body}", response.StatusCode, body);
        throw new HttpRequestException($"Customer cart API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }
}
