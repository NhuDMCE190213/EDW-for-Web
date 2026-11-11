using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models.Review;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffReviewController : Controller
    {
        private readonly ReviewApiClient _reviewApiClient;
        private readonly ILogger<StaffReviewController> _logger;

        public StaffReviewController(ReviewApiClient reviewApiClient, ILogger<StaffReviewController> logger)
        {
            _reviewApiClient = reviewApiClient;
            _logger = logger;
        }

        private string? GetJwtToken() => User.FindFirst("JWTToken")?.Value;

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.1 — VIEW REVIEWS (Admin / Staff)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] StaffReviewListRequest filter)
        {
            filter ??= new StaffReviewListRequest();

            var token = GetJwtToken();
            var reviews = await _reviewApiClient.GetAllReviewsAsync(filter, token);

            // Apply in-memory filtering fallback if API returns full list
            var query = reviews.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(r =>
                    r.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.Content.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.ProductId.HasValue && filter.ProductId.Value > 0)
            {
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.Rating.HasValue && filter.Rating.Value > 0)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(r => r.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.OnlyFlagged == true)
            {
                query = query.Where(r => r.IsFlagged);
            }

            var filteredList = query.OrderByDescending(r => r.CreatedAt).ToList();

            var viewModel = new StaffReviewIndexViewModel
            {
                Reviews = filteredList,
                TotalReviews = reviews.Count,
                TotalFlagged = reviews.Count(r => r.IsFlagged),
                TotalHidden = reviews.Count(r => r.IsHidden),
                AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                Filter = filter
            };

            return View(viewModel);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.2 — FLAG REVIEWS (Admin / Staff)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flag(FlagReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu gắn cờ không hợp lệ. Vui lòng cung cấp lý do.";
                return RedirectToAction(nameof(Index));
            }

            var token = GetJwtToken();
            var flaggedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "Staff";

            var (success, message) = await _reviewApiClient.FlagReviewAsync(request.ReviewId, request.Reason, flaggedBy, token);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unflag(int reviewId)
        {
            var token = GetJwtToken();
            var (success, message) = await _reviewApiClient.UnflagReviewAsync(reviewId, token);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.3 — MODERATE PRODUCT REVIEWS (Admin Only)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(ModerateReviewRequest request)
        {
            // Business Rule FR-01: Only Admin has permission to show or hide customer reviews
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Từ chối quyền truy cập: Chỉ Quản trị viên (Admin) mới có quyền kiểm duyệt ẩn hoặc hiện đánh giá!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Yêu cầu kiểm duyệt không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var token = GetJwtToken();
            var (success, message) = await _reviewApiClient.ModerateReviewAsync(request.ReviewId, request.IsHidden, request.ModerationNote, token);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
