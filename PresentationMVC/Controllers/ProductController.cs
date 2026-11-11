using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ReviewApiClient _reviewApiClient;

        public ProductController(ICartService cartService, ReviewApiClient reviewApiClient)
        {
            _cartService = cartService;
            _reviewApiClient = reviewApiClient;
        }

        // GET: /Product/Index/1 or /Product/Details/1
        public async Task<IActionResult> Index(int id)
        {
            var product = await _cartService.GetProductWithVariantsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var token = User.FindFirst("JWTToken")?.Value;
            product.Reviews = (await _reviewApiClient.GetProductReviewsAsync(id, token))
                .Where(review => string.Equals(review.Status, "Published", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(review => review.CreatedAt)
                .ToList();

            return View(product);
        }

        public async Task<IActionResult> Details(int id)
        {
            return await Index(id);
        }
    }
}
