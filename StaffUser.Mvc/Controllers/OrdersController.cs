using BLL.DTOs.CartItem;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Customer.Mvc.Models;
using System.Security.Claims;

namespace StaffUser.Mvc.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ICartItemService _cartItemService;

        private int CustomerId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public OrdersController(ICustomerService customerService, IOrderService orderService, ICartItemService cartItemService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _cartItemService = cartItemService;
        }

        public async Task<IActionResult> Index()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var customer = await _customerService.GetByEmailAsync(email);
            if (customer == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _orderService.GetOrdersByCustomerIdAsync(customer.CustomerId);
            var orderedOrders = orders
                .OrderByDescending(order => order.CreatedAt)
                .ToList();

            var model = new MyOrdersViewModel
            {
                CustomerName = customer.FullName,
                TotalOrders = orderedOrders.Count,
                TotalSpent = orderedOrders.Sum(order => order.TotalAmount),
                Orders = orderedOrders
            };

            return View(model);
        }

        // POST: Cancel a pending order (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return Json(new { success = false, message = "Invalid order ID." });

            try
            {
                // Verify the order belongs to this customer
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return Json(new { success = false, message = "Order not found." });

                if (order.CustomerId != CustomerId)
                    return Json(new { success = false, message = "You are not authorised to cancel this order." });

                if (order.Status is "Completed" or "Cancelled")
                    return Json(new { success = false, message = $"Cannot cancel an order with status '{order.Status}'." });

                var result = await _orderService.CancelOrderAsync(orderId);
                return Json(new { success = result, message = result ? "Order cancelled successfully." : "Failed to cancel order." });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        // POST: Transfer all items in a Cancelled/Pending order back to cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToCart(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return Json(new { success = false, message = "Invalid order ID." });

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return Json(new { success = false, message = "Order not found." });

                if (order.CustomerId != CustomerId)
                    return Json(new { success = false, message = "You are not authorised to perform this action." });

                if (order.Status != "Cancelled" && order.Status != "Pending")
                    return Json(new { success = false, message = "Only Cancelled or Pending orders can be transferred to cart." });

                int transferred = 0;
                var errors = new List<string>();

                foreach (var item in order.OrderItems)
                {
                    try
                    {
                        var createDto = new CartItemCreateDTO
                        {
                            CustomerId = CustomerId,
                            ProductVariantId = item.ProductVariantId,
                            Quantity = item.Quantity
                        };
                        await _cartItemService.CreateCartItemAsync(createDto);
                        transferred++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{item.ProductName}: {ex.Message}");
                    }
                }

                if (transferred == 0)
                    return Json(new { success = false, message = "No items could be transferred. " + string.Join("; ", errors) });

                var message = transferred == order.OrderItems.Count
                    ? $"All {transferred} item(s) transferred to cart."
                    : $"{transferred}/{order.OrderItems.Count} item(s) transferred. Some items could not be added: {string.Join("; ", errors)}";

                return Json(new { success = true, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}