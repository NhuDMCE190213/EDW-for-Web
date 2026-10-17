using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminStaffController : Controller
    {
        private readonly StaffApiClient _staffApiClient;
        private readonly ILogger<AdminStaffController> _logger;

        public AdminStaffController(StaffApiClient staffApiClient, ILogger<AdminStaffController> logger)
        {
            _staffApiClient = staffApiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 5) pageSize = 10;

                int? currentUserId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
                {
                    currentUserId = parsedId;
                }

                var request = new StaffListRequest
                {
                    SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                    IncludeDelete = false,
                    IncludeDisable = status != "active",
                    IgnoreStaffId = currentUserId,
                    Pagination = new PaginationParams
                    {
                        Page = page,
                        PageSize = pageSize
                    }
                };

                var response = await _staffApiClient.GetStaffsListAsync(request, cancellationToken);
                var items = response.Pagination.Items;

                if (currentUserId.HasValue)
                {
                    items = items.Where(s => s.StaffId != currentUserId.Value).ToList();
                }

                // If status filter is "inactive", filter items where IsActive is false
                if (status == "inactive")
                {
                    items = items.Where(s => !s.IsActive).ToList();
                }

                var viewModel = new StaffIndexViewModel
                {
                    Staffs = items,
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
                _logger.LogError(ex, "Error loading staff list.");
                TempData["ErrorMessage"] = "Failed to load staff list. Please try again.";
                return View(new StaffIndexViewModel { Search = search ?? string.Empty, StatusFilter = status ?? "all" });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new StaffCreateModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffCreateModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _staffApiClient.CreateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = $"Staff member '{model.FullName}' was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create staff member {FullName}", model.FullName);
                var message = ex.Message;
                if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Email), "This email address is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable to create staff member. " + message);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating staff member {FullName}", model.FullName);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var staff = await _staffApiClient.GetByIdAsync(id, cancellationToken);
            if (staff == null)
            {
                TempData["ErrorMessage"] = $"Staff member with ID {id} was not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new StaffUpdateModel
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Email = staff.Email,
                IsActive = staff.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffUpdateModel model, CancellationToken cancellationToken)
        {
            if (id != model.StaffId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _staffApiClient.UpdateAsync(id, model, cancellationToken);
                TempData["SuccessMessage"] = $"Staff member '{model.FullName}' was updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to update staff member ID {Id}", id);
                ModelState.AddModelError(string.Empty, "Unable to update staff member: " + ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating staff member ID {Id}", id);
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
                var newStatus = await _staffApiClient.ToggleStatusAsync(id, cancellationToken);
                return Json(new { success = true, isActive = newStatus, message = $"Status changed to {(newStatus ? "Active" : "Inactive")}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle status for staff member ID {Id}", id);
                return StatusCode(500, new { success = false, message = "Failed to toggle status." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _staffApiClient.DeleteAsync(id, cancellationToken);
                TempData["SuccessMessage"] = "Staff member deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete staff member ID {Id}", id);
                TempData["ErrorMessage"] = "Failed to delete staff member.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
