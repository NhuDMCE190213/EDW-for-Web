using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models.Product.Staff;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    public class StaffProductController : Controller
    {

        private readonly ProductApiClient _api;
        private readonly ILogger<StaffProductController> _logger;

        public StaffProductController(ProductApiClient api, ILogger<StaffProductController> logger)
        {
            _api = api;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var products = await _api.GetAllAsync(cancellationToken);
                return View(products);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not load products from the API");
                TempData["ErrorMessage"] = "Unable to load products from the API.";
                return View(new List<ProductStaffModel>());
            }

        }
    }
}
