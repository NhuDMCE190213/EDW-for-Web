using PresentationMVC.Models;
using PresentationMVC.Models.Cart;
using PresentationMVC.Models.Order.Customer;

namespace PresentationMVC.Services
{
    public interface ICartService
    {
        Task<List<CartItemDto>> GetCartItemsAsync(int customerId, CancellationToken cancellationToken = default);
        Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default);
        Task<CartItemDto> AddToCartAsync(int customerId, Guid variantId, int quantity, CancellationToken cancellationToken = default);
        Task<bool> UpdateQuantityAsync(int customerId, int cartItemId, int quantity, CancellationToken cancellationToken = default);
        Task<bool> RemoveItemAsync(int customerId, int cartItemId, CancellationToken cancellationToken = default);
        Task<OrderCheckoutResultDto> CheckoutAsync(int customerId, List<int> selectedCartItemIds, CancellationToken cancellationToken = default);
        Task<TransferResult> TransferOrderToCartAsync(int customerId, Guid orderId, CancellationToken cancellationToken = default);
        Task<ProductCustomerModel?> GetProductWithVariantsAsync(int productId, CancellationToken cancellationToken = default);
        Task<List<ProductCustomerModel>> GetCustomerProductsAsync(string? query = null, string? category = null, CancellationToken cancellationToken = default);
    }
}
