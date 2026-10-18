using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers;

[Authorize(Roles = "Customer")]
public sealed class CartController : Controller
{
    private readonly CustomerCartApiClient _cartApi;
    private readonly ILogger<CartController> _logger;

    public CartController(CustomerCartApiClient cartApi, ILogger<CartController> logger)
    {
        _cartApi = cartApi;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(CartItemCreateModel model, int productId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please select a valid product variant and quantity.";
            return RedirectToAction("ProductDetails", "Home", new { id = productId });
        }

        var customerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(customerIdValue, out var customerId) || customerId <= 0)
        {
            return Challenge();
        }

        model.CustomerId = customerId;

        try
        {
            await _cartApi.AddAsync(model, cancellationToken);
            TempData["SuccessMessage"] = "Product added to cart successfully.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not add variant {VariantId} to cart for customer {CustomerId}",
                model.ProductVariantId, customerId);
            TempData["ErrorMessage"] = "The product could not be added to your cart.";
        }

        return RedirectToAction("ProductDetails", "Home", new { id = productId });
    }
}
