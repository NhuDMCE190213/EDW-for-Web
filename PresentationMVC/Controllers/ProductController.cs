using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ICartService _cartService;

        public ProductController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: /Product/Index/1 or /Product/Details/1
        public async Task<IActionResult> Index(int id)
        {
            var product = await _cartService.GetProductWithVariantsAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> Details(int id)
        {
            return await Index(id);
        }
    }
}
