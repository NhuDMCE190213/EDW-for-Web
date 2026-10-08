using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;
using PresentationMVC.Models.Order.Staff;
using Microsoft.AspNetCore.Authorization;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Staff,Admin")]
    public class StaffOrderController : Controller
    {
        private readonly OrderApiClient _orderApiClient;

        public StaffOrderController(OrderApiClient orderApiClient)
        {
            _orderApiClient = orderApiClient;
        }

        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            if (User.IsInRole("Staff"))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = await _orderApiClient.GetDashboardAsync(cancellationToken);
            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderApiClient.GetAllAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(Guid orderId, string status)
        {
            var success = await _orderApiClient.UpdateStatusAsync(orderId, status);
            if (!success)
            {
                TempData["Error"] = "Failed to update order status.";
            }
            else
            {
                TempData["Success"] = "Order status updated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
