using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models.Order.Customer;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    // Removing [Authorize] for now, or you can keep it if PresentationMVC implements Auth later.
    // Assuming user gets CustomerId via claims. Since we may not have Auth configured in PresentationMVC yet,
    // we'll leave [Authorize] but provide a fallback if the claims are missing.
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly CustomerOrderApiClient _orderApiClient;

        private int CustomerId 
        {
            get
            {
                var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(val, out int id)) return id;
                return 0; // Or handle unauthorized access
            }
        }

        public OrdersController(CustomerOrderApiClient orderApiClient)
        {
            _orderApiClient = orderApiClient;
        }

        public async Task<IActionResult> Index()
        {
            // For now, since PresentationMVC doesn't have login, let's allow it to fallback or redirect to Login
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                // In case it's not authenticated yet
                return RedirectToAction("Login", "Auth");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Email";
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value ?? email;

            var orders = await _orderApiClient.GetMyOrdersAsync(customerId);
            var orderedOrders = orders
                .OrderByDescending(order => order.CreatedAt)
                .ToList();

            var model = new MyOrdersViewModel
            {
                CustomerName = fullName,
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
                var result = await _orderApiClient.CancelOrderAsync(orderId);
                return Json(new { success = result, message = result ? "Order cancelled successfully." : "Failed to cancel order." });
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
                var result = await _orderApiClient.TransferToCartAsync(orderId, CustomerId);
                
                if (result == null || !result.Success)
                    return Json(new { success = false, message = "Failed to transfer items to cart." });

                var message = result.Transferred == result.Total
                    ? $"All {result.Transferred} item(s) transferred to cart."
                    : $"{result.Transferred}/{result.Total} item(s) transferred. Some items could not be added: {string.Join("; ", result.Errors)}";

                return Json(new { success = true, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}
