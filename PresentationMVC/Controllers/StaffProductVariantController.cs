using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers;

public sealed class StaffProductVariantController : Controller
{
    private readonly ProductVariantApiClient _api;
    private readonly ILogger<StaffProductVariantController> _logger;

    public StaffProductVariantController(ProductVariantApiClient api, ILogger<StaffProductVariantController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int productId, CancellationToken cancellationToken)
    {
        try
        {
            var variants = await _api.GetByProductAsync(productId, cancellationToken);
            ViewBag.ProductId = productId;
            return View(variants);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not load variants for product {ProductId}", productId);
            TempData["ErrorMessage"] = "Unable to load product variants from the API.";
            return View(new List<ProductVariantModel>());
        }
    }

    [HttpGet]
    public IActionResult Create(int productId) =>
        View(new ProductVariantCreateModel { ProductId = productId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductVariantCreateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _api.CreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = "The variant was created successfully!";
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not create variant for product {ProductId}", model.ProductId);
            ModelState.AddModelError(string.Empty, "The API could not create this variant.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int productId, CancellationToken cancellationToken)
    {
        var variant = await _api.GetAsync(id, cancellationToken);
        if (variant is null)
        {
            return NotFound();
        }

        return View(new ProductVariantUpdateModel
        {
            ProductVariantId = variant.ProductVariantId,
            ProductId = productId,
            Color = variant.Color,
            Cpu = variant.Cpu,
            Ram = variant.Ram,
            Storage = variant.Storage,
            ScreenSize = variant.ScreenSize,
            Price = variant.Price,
            StockQuantity = variant.StockQuantity,
            ImageUrl = variant.ImageUrl,
            PromotionId = variant.PromotionId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductVariantUpdateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _api.UpdateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = "The variant was updated successfully!";
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not update variant {VariantId}", model.ProductVariantId);
            ModelState.AddModelError(string.Empty, "The API could not update this variant.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, int productId, CancellationToken cancellationToken)
    {
        try
        {
            await _api.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "The variant was deleted successfully!";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not delete variant {VariantId}", id);
            TempData["ErrorMessage"] = "The API could not delete this variant.";
        }

        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockIn(Guid id, int productId, int increaseAmount, CancellationToken cancellationToken)
    {
        if (increaseAmount <= 0)
        {
            TempData["ErrorMessage"] = "Stock-in amount must be greater than zero.";
            return RedirectToAction(nameof(Index), new { productId });
        }

        try
        {
            await _api.StockInAsync(id, increaseAmount, cancellationToken);
            TempData["SuccessMessage"] = "Stock in successful!";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not stock in variant {VariantId}", id);
            TempData["ErrorMessage"] = "The API could not stock in this variant.";
        }

        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpGet]
    public IActionResult CreateVariants(int productId) =>
        View(new ProductVariantMatrixModel { ProductId = productId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVariants(ProductVariantMatrixModel model, CancellationToken cancellationToken)
    {
        if (model.Variants.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one variant.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            foreach (var variant in model.Variants)
            {
                await _api.CreateAsync(new ProductVariantCreateModel
                {
                    ProductId = model.ProductId,
                    Color = variant.Color,
                    Cpu = variant.Cpu,
                    Ram = variant.Ram,
                    Storage = variant.Storage,
                    ScreenSize = variant.ScreenSize,
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity
                }, cancellationToken);
            }

            TempData["SuccessMessage"] = "The variants were created successfully!";
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not create variant matrix for product {ProductId}", model.ProductId);
            ModelState.AddModelError(string.Empty, "The API could not create one or more variants.");
            return View(model);
        }
    }
}
