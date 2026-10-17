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
        private readonly ICartService _cartService;

        public HomeController(
            ILogger<HomeController> logger,
            ProductApiClient productApi,
            CategoryApiClient categoryApi,
            OrderApiClient orderApi,
            ICartService cartService)
        {
            _logger = logger;
            _productApi = productApi;
            _categoryApi = categoryApi;
            _orderApi = orderApi;
            _cartService = cartService;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load admin dashboard data");
                TempData["ErrorMessage"] = "Unable to load dashboard data from the API.";
                return View(new AdminDashboardViewModel());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Customer(string? q, string? category, CancellationToken cancellationToken)
        {
            var products = await _cartService.GetCustomerProductsAsync(q, category, cancellationToken);
            
            List<CategoryModel> categories = new();
            try
            {
                var apiCats = await _categoryApi.GetAllAsync(cancellationToken);
                if (apiCats != null && apiCats.Count > 0)
                {
                    categories = apiCats.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();
                }
            }
            catch
            {
                // Fallback default categories
            }

            if (categories.Count == 0)
            {
                categories = new List<CategoryModel>
                {
                    new() { Id = 1, Name = "Laptops" },
                    new() { Id = 2, Name = "Phones" },
                    new() { Id = 3, Name = "Tablets" },
                    new() { Id = 4, Name = "Accessories" }
                };
            }

            return View(new CustomerStoreViewModel
            {
                Products = products,
                Categories = categories,
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
