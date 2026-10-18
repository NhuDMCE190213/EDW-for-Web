using System.Net.Http.Json;
using PresentationMVC.Models;

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
