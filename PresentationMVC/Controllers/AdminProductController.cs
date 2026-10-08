using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresentationMVC.Models;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly ProductApiClient _api;
        private readonly CategoryApiClient _categoryApi;
        private readonly ILogger<AdminProductController> _logger;

        public AdminProductController(ProductApiClient api, CategoryApiClient categoryApi, ILogger<AdminProductController> logger)
        {
            _api = api;
            _categoryApi = categoryApi;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int categoryFilter = 0, int? editId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _api.GetAllAsync(cancellationToken);
                var categories = await _categoryApi.GetAllAsync(cancellationToken);
                var filtered = products
                    .Where(p => !p.IsDeleted &&
                        (categoryFilter == 0 || p.CategoryId == categoryFilter) &&
                        (string.IsNullOrWhiteSpace(search) ||
                         p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                         p.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(p => p.ProductId)
                    .ToList();
                var editing = editId.HasValue ? products.FirstOrDefault(p => p.ProductId == editId.Value) : null;

                return View(new AdminProductIndexViewModel
                {
                    Products = filtered,
                    Categories = categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList(),
                    Form = editing is null ? new ProductStaffUpdateModel() : new ProductStaffUpdateModel
                    {
                        ProductId = editing.ProductId,
                        ProductName = editing.ProductName,
                        Brand = editing.Brand,
                        CategoryId = editing.CategoryId,
                        ImageUrl = editing.ImageUrl
                    },
                    IsEditing = editing is not null,
                    Search = search ?? string.Empty,
                    CategoryFilter = categoryFilter
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not load products");
                TempData["ErrorMessage"] = "Unable to load products from the API.";
                return View(new AdminProductIndexViewModel { Search = search ?? string.Empty, CategoryFilter = categoryFilter });
            }
        }

        [HttpGet]
        public IActionResult Create() => RedirectToAction(nameof(Index));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Form")] ProductStaffCreateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a product name, brand, and category.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _api.CreateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "The product was created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not create product {ProductName}", model.ProductName);
                ModelState.AddModelError(string.Empty, "The API could not create this product.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int productId, CancellationToken cancellationToken)
        {
            var product = await _api.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index), new { editId = product.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind(Prefix = "Form")] ProductStaffUpdateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a product name, brand, and category.";
                return RedirectToAction(nameof(Index), new { editId = model.ProductId });
            }

            try
            {
                await _api.UpdateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "The product was updated successfully!";
                return RedirectToAction(nameof(Index), new { editId = model.ProductId });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not update product {ProductId}", model.ProductId);
                ModelState.AddModelError(string.Empty, "The API could not update this product.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int productId, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync(productId, cancellationToken);
                TempData["SuccessMessage"] = "The product was deleted successfully!";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not delete product {ProductId}", productId);
                TempData["ErrorMessage"] = "The API could not delete this product.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
