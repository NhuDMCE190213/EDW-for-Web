using PresentationMVC.Models;
using PresentationMVC.Models.Cart;
using PresentationMVC.Models.Order.Customer;

namespace PresentationMVC.Services;

public sealed class CartService : ICartService
{
    private readonly ProductApiClient _productApi;
    private readonly ProductVariantApiClient _variantApi;
    private readonly CustomerCartApiClient _customerCartApi;
    private readonly CustomerOrderApiClient _customerOrderApi;

    public CartService(
        ProductApiClient productApi,
        ProductVariantApiClient variantApi,
        CustomerCartApiClient customerCartApi,
        CustomerOrderApiClient customerOrderApi)
    {
        _productApi = productApi;
        _variantApi = variantApi;
        _customerCartApi = customerCartApi;
        _customerOrderApi = customerOrderApi;
    }

    public Task<List<CartItemDto>> GetCartItemsAsync(
        int customerId, CancellationToken cancellationToken = default)
        => _customerCartApi.GetAsync(customerId, cancellationToken);

    public async Task<int> GetCartItemCountAsync(
        int customerId, CancellationToken cancellationToken = default)
        => (await GetCartItemsAsync(customerId, cancellationToken)).Sum(item => item.Quantity);

    public async Task<CartItemDto> AddToCartAsync(
        int customerId,
        Guid variantId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        await _customerCartApi.AddAsync(new CartItemCreateModel
        {
            CustomerId = customerId,
            ProductVariantId = variantId,
            Quantity = quantity
        }, cancellationToken);

        var items = await _customerCartApi.GetAsync(customerId, cancellationToken);
        return items.LastOrDefault(item => item.ProductVariantId == variantId)
            ?? throw new InvalidOperationException("The cart API did not return the added item.");
    }

    public async Task<bool> UpdateQuantityAsync(
        int customerId,
        int cartItemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var items = await _customerCartApi.GetAsync(customerId, cancellationToken);
        if (!items.Any(item => item.CartItemId == cartItemId))
        {
            return false;
        }

        if (quantity <= 0)
        {
            await _customerCartApi.DeleteAsync(cartItemId, cancellationToken);
        }
        else
        {
            await _customerCartApi.UpdateAsync(new CartItemUpdateDto
            {
                CartItemId = cartItemId,
                Quantity = quantity
            }, cancellationToken);
        }

        return true;
    }

    public async Task<bool> RemoveItemAsync(
        int customerId,
        int cartItemId,
        CancellationToken cancellationToken = default)
    {
        var items = await _customerCartApi.GetAsync(customerId, cancellationToken);
        if (!items.Any(item => item.CartItemId == cartItemId))
        {
            return false;
        }

        await _customerCartApi.DeleteAsync(cartItemId, cancellationToken);
        return true;
    }

    public Task<OrderCheckoutResultDto> CheckoutAsync(
        int customerId,
        List<int> selectedCartItemIds,
        CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            throw new ArgumentException("Customer must be logged in to checkout.", nameof(customerId));
        }

        if (selectedCartItemIds.Count == 0)
        {
            throw new ArgumentException("Please select at least one item to proceed.");
        }

        return _customerOrderApi.CheckoutAsync(customerId, selectedCartItemIds, cancellationToken);
    }

    public async Task<TransferResult> TransferOrderToCartAsync(
        int customerId,
        Guid orderId,
        CancellationToken cancellationToken = default)
        => await _customerOrderApi.TransferToCartAsync(orderId, customerId, cancellationToken)
            ?? throw new HttpRequestException("Customer transfer API returned an empty response.");

    public async Task<ProductCustomerModel?> GetProductWithVariantsAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        var product = await _productApi.GetCustomerByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Variants = await _variantApi.GetCustomerByProductAsync(productId, cancellationToken);
        return product;
    }

    public async Task<List<ProductCustomerModel>> GetCustomerProductsAsync(
        string? query = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var products = await _productApi.GetCustomerAllAsync(cancellationToken);
        var variantsByProduct = await Task.WhenAll(products.Select(async product =>
            (Product: product,
                Variants: await _variantApi.GetCustomerByProductAsync(product.ProductId, cancellationToken))));

        var result = variantsByProduct.Select(item =>
        {
            item.Product.Variants = item.Variants;
            return item.Product;
        });

        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(product =>
                product.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                product.Brand.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            result = result.Where(product =>
                product.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase));
        }

        return result.ToList();
    }
}
