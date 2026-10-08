using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresentationMVC.Models;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly CategoryApiClient _api;
        private readonly ILogger<AdminCategoryController> _logger;
        public AdminCategoryController(CategoryApiClient api, ILogger<AdminCategoryController> logger)
        {
            _api = api;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? editId, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _api.GetAllAsync(cancellationToken);
                var filtered = categories
                    .Where(c => !c.IsDeleted &&
                        (string.IsNullOrWhiteSpace(search) ||
                         c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(c => c.Id)
                    .ToList();
                var editing = editId.HasValue
                    ? categories.FirstOrDefault(c => c.Id == editId.Value)
                    : null;

                return View(new AdminCategoryIndexViewModel
                {
                    Categories = filtered,
                    Form = editing is null
                        ? new CategoryUpdateModel()
                        : new CategoryUpdateModel { Id = editing.Id, Name = editing.Name },
                    IsEditing = editing is not null,
                    Search = search ?? string.Empty
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not load categories");
                TempData["ErrorMessage"] = "Unable to load categories from the API.";
                return View(new AdminCategoryIndexViewModel { Search = search ?? string.Empty });
            }
        }

        [HttpGet]
        public IActionResult Create() => RedirectToAction(nameof(Index));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Form")] CategoryCreateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _api.CreateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "The category was created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not create category {CategoryName}", model.Name);
                ModelState.AddModelError(string.Empty, "The API could not create this category.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int categoryId, CancellationToken cancellationToken)
        {
            var category = await _api.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index), new { editId = category.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind(Prefix = "Form")] CategoryUpdateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(Index), new { editId = model.Id });
            }

            try
            {
                await _api.UpdateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "The category was updated successfully!";
                return RedirectToAction(nameof(Index), new { editId = model.Id });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not update category {CategoryId}", model.Id);
                ModelState.AddModelError(string.Empty, "The API could not update this category.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int categoryId, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync(categoryId, cancellationToken);
                TempData["SuccessMessage"] = "The category was deleted successfully!";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not delete category {CategoryId}", categoryId);
                TempData["ErrorMessage"] = "The API could not delete this category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
