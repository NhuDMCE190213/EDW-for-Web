using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly OrderApiClient _orderApiClient;

        public AdminOrderController(OrderApiClient orderApiClient)
        {
            _orderApiClient = orderApiClient;
        }

        // GET: /AdminOrder/Dashboard
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var model = await _orderApiClient.GetDashboardAsync(cancellationToken);
            return View("~/Views/StaffOrder/Dashboard.cshtml", model);
        }

        // GET: /AdminOrder/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _orderApiClient.GetAllAsync();
            return View("~/Views/StaffOrder/Index.cshtml", orders);
        }
    }
}
