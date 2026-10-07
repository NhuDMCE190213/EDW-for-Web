using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;
using PresentationMVC.Models.Order.Staff;

namespace PresentationMVC.Controllers
{
    public class StaffOrderController : Controller
    {
        private readonly OrderApiClient _orderApiClient;

        public StaffOrderController(OrderApiClient orderApiClient)
        {
            _orderApiClient = orderApiClient;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = await _orderApiClient.GetDashboardAsync();
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
