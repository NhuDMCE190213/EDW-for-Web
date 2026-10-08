using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresentationMVC.Models;
using PresentationMVC.Services;
using PresentationMVC.Models.Order.Staff;
using System.Diagnostics;

namespace PresentationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductApiClient _productApi;
        private readonly CategoryApiClient _categoryApi;
        private readonly OrderApiClient _orderApi;

        public HomeController(
            ILogger<HomeController> logger,
            ProductApiClient productApi,
            CategoryApiClient categoryApi,
            OrderApiClient orderApi)
        {
            _logger = logger;
            _productApi = productApi;
            _categoryApi = categoryApi;
            _orderApi = orderApi;
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (User.IsInRole("Staff"))
            {
                var orderDashboard = await _orderApi.GetDashboardAsync(cancellationToken);
                return View("StaffDashboard", orderDashboard);
            }

            try
            {
                var productsTask = _productApi.GetAllAsync(cancellationToken);
                var categoriesTask = _categoryApi.GetAllAsync(cancellationToken);
                await Task.WhenAll(productsTask, categoriesTask);

                return View(new AdminDashboardViewModel
                {
                    Products = productsTask.Result.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedAt).ToList(),
                    Categories = categoriesTask.Result.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList()
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not load admin dashboard data");
                TempData["ErrorMessage"] = "Unable to load dashboard data from the API.";
                return View(new AdminDashboardViewModel());
            }
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Customer(string? q, string? category, CancellationToken cancellationToken)
        {
            var productsTask = _productApi.GetAllAsync(cancellationToken);
            var categoriesTask = _categoryApi.GetAllAsync(cancellationToken);
            await Task.WhenAll(productsTask, categoriesTask);

            var products = productsTask.Result.Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(q, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            return View(new CustomerStoreViewModel
            {
                Products = products.OrderBy(p => p.ProductId).ToList(),
                Categories = categoriesTask.Result.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList(),
                Search = q ?? string.Empty,
                Category = category ?? string.Empty
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
