using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        private int CustomerId
        {
            get
            {
                var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(val, out int id)) return id;
                return 1; // Fallback customer ID if testing without auth
            }
        }

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Cart or /Cart/Index
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("CustomerLogin", "Auth", new { returnUrl = "/Cart" });
            }

            var cart = await _cartService.GetCartItemsAsync(CustomerId);
            return View(cart);
        }

        // GET: /Cart/GetCartCount (Used by navbar badge)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { count = 0 });
            }

            var count = await _cartService.GetCartItemCountAsync(CustomerId);
            return Json(new { count });
        }

        // GET: /Cart/GetProductVariants?productId=123 (Used by quick selector modal)
        [HttpGet]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            var product = await _cartService.GetProductWithVariantsAsync(productId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            var activeVariants = product.Variants.Where(v => v.StockQuantity >= 0).ToList();

            return Json(new
            {
                success = true,
                productId = product.ProductId,
                productName = product.ProductName,
                brand = product.Brand,
                imageUrl = product.ImageUrl,
                variants = activeVariants.Select(v => new
                {
                    productVariantId = v.ProductVariantId,
                    sku = v.Sku,
                    color = v.Color,
                    cpu = v.Cpu,
                    ram = v.Ram,
                    storage = v.Storage,
                    screenSize = v.ScreenSize,
                    price = v.Price,
                    finalPrice = v.Price,
                    isOnSale = false,
                    stockQuantity = v.StockQuantity,
                    imageUrl = v.ImageUrl ?? product.ImageUrl
                })
            });
        }

        // POST: /Cart/AddToCart (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid variantId, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { success = false, message = "Please sign in to add products to your cart." });
            }

            try
            {
                var addedItem = await _cartService.AddToCartAsync(CustomerId, variantId, quantity);
                var totalCount = await _cartService.GetCartItemCountAsync(CustomerId);

                return Json(new
                {
                    success = true,
                    message = $"Added {quantity} × \"{addedItem.ProductVariant?.ProductName ?? "Item"}\" to your cart!",
                    totalCount
                });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddToCart failed");
                return Json(new { success = false, message = "An error occurred while adding to cart: " + ex.Message });
            }
        }

        // POST: /Cart/UpdateQuantity (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var success = await _cartService.UpdateQuantityAsync(CustomerId, cartItemId, quantity);
                if (!success)
                {
                    return Json(new { success = false, message = "Item not found in cart." });
                }

                var cart = await _cartService.GetCartItemsAsync(CustomerId);
                var updatedItem = cart.FirstOrDefault(i => i.CartItemId == cartItemId);
                var totalCount = cart.Sum(i => i.Quantity);
                var cartTotal = cart.Sum(i => i.Quantity * (i.ProductVariant?.Price ?? 0));
                var totalPrice = (updatedItem?.Quantity ?? 0) * (updatedItem?.ProductVariant?.Price ?? 0);

                return Json(new
                {
                    success = true,
                    totalCount,
                    totalPrice,
                    cartTotal
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateQuantity failed");
                return Json(new { success = false, message = "Cannot update quantity: " + ex.Message });
            }
        }

        // POST: /Cart/Remove (AJAX)
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var removed = await _cartService.RemoveItemAsync(CustomerId, cartItemId);
                var cart = await _cartService.GetCartItemsAsync(CustomerId);
                var totalCount = cart.Sum(i => i.Quantity);
                var cartTotal = cart.Sum(i => i.Quantity * (i.ProductVariant?.Price ?? 0));

                return Json(new
                {
                    success = removed,
                    totalCount,
                    cartTotal,
                    itemsLeft = cart.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remove cart item failed");
                return Json(new { success = false, message = "Cannot remove item: " + ex.Message });
            }
        }

        // POST: /Cart/Checkout or /Cart/TransferToOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(List<int> selectedCartItemIds)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("CustomerLogin", "Auth", new { returnUrl = "/Cart" });
            }

            if (selectedCartItemIds == null || selectedCartItemIds.Count == 0)
            {
                TempData["CheckoutError"] = "Please select at least one item to proceed to order.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _cartService.CheckoutAsync(CustomerId, selectedCartItemIds);
                return View("~/Views/Order/CheckoutSuccess.cshtml", result);
            }
            catch (Exception ex)
            {
                TempData["CheckoutError"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Cart/TransferToOrder (Alias for Transfer to Order requirement)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToOrder(List<int> selectedCartItemIds)
        {
            return await Checkout(selectedCartItemIds);
        }
    }
}
