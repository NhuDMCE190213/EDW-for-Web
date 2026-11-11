using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models.Review;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ReviewApiClient _reviewApiClient;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ReviewApiClient reviewApiClient, ILogger<ReviewsController> logger)
        {
            _reviewApiClient = reviewApiClient;
            _logger = logger;
        }

        private int CustomerId
        {
            get
            {
                var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(val, out int id)) return id;
                return 0;
            }
        }

        private string? GetJwtToken() => User.FindFirst("JWTToken")?.Value;

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.1 – VIEW MY REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> MyReviews(int? rating, string? status, string? search, int page = 1)
        {
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                return RedirectToAction("CustomerLogin", "Auth", new { returnUrl = Url.Action("MyReviews") });
            }

            var token = GetJwtToken();
            var customerName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "Customer";

            try
            {
                var allReviews = await _reviewApiClient.GetMyReviewsAsync(customerId, token);

                // Apply in-memory filtering (rating, status, search)
                var query = allReviews.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim();
                    query = query.Where(r =>
                        r.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        r.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                }

                if (rating.HasValue && rating.Value > 0)
                {
                    query = query.Where(r => r.Rating == rating.Value);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                }

                var filteredList = query.OrderByDescending(r => r.CreatedAt).ToList();

                // Pagination
                const int pageSize = 6;
                var totalItems = filteredList.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                if (totalPages < 1) totalPages = 1;
                if (page < 1) page = 1;
                if (page > totalPages) page = totalPages;

                var pagedList = filteredList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new MyReviewsViewModel
                {
                    CustomerName = customerName,
                    TotalReviews = totalItems,
                    Reviews = pagedList,
                    FilterRating = rating,
                    FilterStatus = status,
                    SearchTerm = search,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reviews for customer {CustomerId}", customerId);
                TempData["ErrorMessage"] = "Unable to load your reviews. Please try again later.";
                return View(new MyReviewsViewModel
                {
                    CustomerName = customerName,
                    Reviews = new List<CustomerReviewDto>()
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.2 – CREATE REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Create(int productId, string? productName = null, Guid? orderId = null, int? orderItemId = null)
        {
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (productId <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction("MyReviews");
            }

            // BR-REVIEW-04: Prevent duplicate review submission
            var token = GetJwtToken();
            try
            {
                var existingReviews = await _reviewApiClient.GetMyReviewsAsync(customerId, token);
                var alreadyReviewed = existingReviews.Any(r => r.ProductId == productId &&
                    (!orderId.HasValue || !r.OrderId.HasValue || r.OrderId == orderId));

                if (alreadyReviewed)
                {
                    TempData["ErrorMessage"] = "Bạn đã gửi đánh giá cho sản phẩm này trong đơn hàng trước đó.";
                    return RedirectToAction("MyReviews");
                }
            }
            catch
            {
                // Proceed if check fails to not block user
            }

            var model = new CreateReviewViewModel
            {
                ProductId = productId,
                ProductName = string.IsNullOrWhiteSpace(productName) ? $"Sản phẩm #{productId}" : productName,
                OrderId = orderId,
                OrderItemId = orderItemId,
                Rating = 5
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = GetJwtToken();
            var (success, message) = await _reviewApiClient.CreateReviewAsync(customerId, model, token);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(MyReviews));
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.3 – UPDATE REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            var token = GetJwtToken();
            var review = await _reviewApiClient.GetReviewByIdAsync(id, token);

            if (review == null)
            {
                // Fallback: check my reviews list
                var myReviews = await _reviewApiClient.GetMyReviewsAsync(customerId, token);
                review = myReviews.FirstOrDefault(r => r.ReviewId == id);
            }

            if (review == null)
            {
                TempData["ErrorMessage"] = "Đánh giá không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(MyReviews));
            }

            // BR-REVIEW-10: Review Update Restriction
            if (review.IsLockedByAdmin || review.Status.Equals("Hidden", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Đánh giá này đã bị quản trị viên khóa, bạn không thể chỉnh sửa.";
                return RedirectToAction(nameof(MyReviews));
            }

            var model = new UpdateReviewViewModel
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                ProductName = review.ProductName,
                Rating = review.Rating,
                Content = review.Content,
                IsLocked = review.IsLockedByAdmin
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateReviewViewModel model)
        {
            var customerId = CustomerId;
            if (customerId <= 0)
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = GetJwtToken();
            var (success, message) = await _reviewApiClient.UpdateReviewAsync(customerId, model, token);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(MyReviews));
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
    }
}
