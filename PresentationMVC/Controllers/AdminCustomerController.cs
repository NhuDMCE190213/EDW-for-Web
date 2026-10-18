using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCustomerController : Controller
    {
        private readonly CustomerApiClient _customerApiClient;
        private readonly ILogger<AdminCustomerController> _logger;

        public AdminCustomerController(CustomerApiClient customerApiClient, ILogger<AdminCustomerController> logger)
        {
            _customerApiClient = customerApiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 5) pageSize = 10;

                var request = new CustomerListRequest
                {
                    SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                    IncludeDelete = false,
                    IncludeDisable = status != "active",
                    Pagination = new PaginationParams
                    {
                        Page = page,
                        PageSize = pageSize
                    }
                };

                var response = await _customerApiClient.GetCustomersListAsync(request, cancellationToken);
                var items = response.Pagination.Items;

                if (status == "inactive")
                {
                    items = items.Where(c => !c.IsActive).ToList();
                }

                var viewModel = new CustomerIndexViewModel
                {
                    Customers = items,
                    TotalCount = status == "inactive" ? items.Count : response.Pagination.TotalCount,
                    ActiveCount = response.ActiveCount,
                    Page = response.Pagination.Page > 0 ? response.Pagination.Page : page,
                    PageSize = response.Pagination.PageSize > 0 ? response.Pagination.PageSize : pageSize,
                    Search = search ?? string.Empty,
                    StatusFilter = status ?? "all"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer list.");
                TempData["ErrorMessage"] = "Failed to load customer list. Please try again.";
                return View(new CustomerIndexViewModel { Search = search ?? string.Empty, StatusFilter = status ?? "all" });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CustomerCreateModel { IsActive = true, Points = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _customerApiClient.CreateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = $"Customer '{model.FullName}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create customer {FullName}", model.FullName);
                var message = ex.Message;
                if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Email), "This email address is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable to create customer: " + message);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating customer {FullName}", model.FullName);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var customer = await _customerApiClient.GetByIdAsync(id, cancellationToken);
            if (customer == null)
            {
                TempData["ErrorMessage"] = $"Customer with ID {id} was not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CustomerUpdateModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Points = customer.Points,
                IsActive = customer.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerUpdateModel model, CancellationToken cancellationToken)
        {
            if (id != model.CustomerId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _customerApiClient.UpdateAsync(id, model, cancellationToken);
                TempData["SuccessMessage"] = $"Customer '{model.FullName}' was updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to update customer ID {Id}", id);
                ModelState.AddModelError(string.Empty, "Unable to update customer: " + ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating customer ID {Id}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Enabled for AJAX JSON toggle
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
        {
            try
            {
                var newStatus = await _customerApiClient.ToggleStatusAsync(id, cancellationToken);
                return Json(new { success = true, isActive = newStatus, message = $"Status changed to {(newStatus ? "Active" : "Inactive")}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle status for customer ID {Id}", id);
                return StatusCode(500, new { success = false, message = "Failed to toggle status." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _customerApiClient.DeleteAsync(id, cancellationToken);
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete customer ID {Id}", id);
                TempData["ErrorMessage"] = "Failed to delete customer.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
