using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Customer.Mvc.Models;
using System.Security.Claims;
using StaffUser.Mvc.Services;
using BLL.Services.Interfaces;

namespace StaffUser.Mvc.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerOrderApiClient _orderApiClient;

        private int CustomerId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public OrdersController(ICustomerService customerService, CustomerOrderApiClient orderApiClient)
        {
            _customerService = customerService;
            _orderApiClient = orderApiClient;
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

            var orders = await _orderApiClient.GetMyOrdersAsync(customer.CustomerId);
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