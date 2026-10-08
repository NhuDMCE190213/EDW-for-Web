using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresentationMVC.Models;
using PresentationMVC.Services;
using System.Diagnostics;

namespace PresentationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductApiClient _productApi;
        private readonly CategoryApiClient _categoryApi;

        public HomeController(
            ILogger<HomeController> logger,
            ProductApiClient productApi,
            CategoryApiClient categoryApi)
        {
            _logger = logger;
            _productApi = productApi;
            _categoryApi = categoryApi;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
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
