using PresentationMVC.Models.Review;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PresentationMVC.Services
{
    public sealed class ReviewApiClient
    {
        private const string StaffResource = "api/staff/reviews";
        private const string AdminResource = "api/admin/reviews";
        private const string CustomerResource = "api/customer/reviews";

        private readonly HttpClient _httpClient;
        private readonly ILogger<ReviewApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReviewApiClient(HttpClient httpClient, ILogger<ReviewApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private void AttachBearerToken(HttpRequestMessage request, string? jwtToken)
        {
            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.1 — VIEW REVIEWS (Admin / Staff)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<List<ReviewStaffDto>> GetAllReviewsAsync(StaffReviewListRequest? filter = null, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var queryParams = new List<string>();
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                        queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
                    if (filter.ProductId.HasValue)
                        queryParams.Add($"productId={filter.ProductId.Value}");
                    if (filter.Rating.HasValue)
                        queryParams.Add($"rating={filter.Rating.Value}");
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                        queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
                    if (filter.OnlyFlagged.HasValue && filter.OnlyFlagged.Value)
                        queryParams.Add("onlyFlagged=true");
                }

                var uri = StaffResource + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ReviewStaffDto>>(_jsonOptions, cancellationToken) ?? new();
                }

                _logger.LogWarning("GetAllReviewsAsync returned status code {StatusCode}", response.StatusCode);
                return new List<ReviewStaffDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve reviews from API");
                return new List<ReviewStaffDto>();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.2 — FLAG REVIEWS (Admin / Staff)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> FlagReviewAsync(int reviewId, string reason, string? flaggedBy = null, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{StaffResource}/{reviewId}/flag")
                {
                    Content = JsonContent.Create(new { reason, flaggedBy })
                };
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Gắn cờ đánh giá thành công.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return (false, "Đánh giá này đã được gắn cờ trước đó.");
                }

                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(errorMsg) ? "Không thể gắn cờ đánh giá." : errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging review {ReviewId}", reviewId);
                return (false, "Lỗi kết nối máy chủ khi gắn cờ đánh giá.");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-06.3 — MODERATE PRODUCT REVIEWS (Admin Only)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> ModerateReviewAsync(int reviewId, bool isHidden, string? note = null, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{AdminResource}/{reviewId}/moderate")
                {
                    Content = JsonContent.Create(new { isHidden, note })
                };
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var actionText = isHidden ? "Ẩn" : "Hiển thị lại";
                    return (true, $"{actionText} đánh giá thành công.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Đánh giá không tồn tại hoặc đã bị xóa.");
                }

                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(errorMsg) ? "Không thể cập nhật trạng thái kiểm duyệt." : errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moderating review {ReviewId}", reviewId);
                return (false, "Lỗi kết nối máy chủ khi kiểm duyệt đánh giá.");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.1 — VIEW MY REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<List<CustomerReviewDto>> GetMyReviewsAsync(int customerId, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{CustomerResource}?customerId={customerId}");
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CustomerReviewDto>>(_jsonOptions, cancellationToken) ?? new();
                }

                _logger.LogWarning("GetMyReviewsAsync returned status code {StatusCode}", response.StatusCode);
                return new List<CustomerReviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve customer reviews for CustomerId {CustomerId}", customerId);
                throw new HttpRequestException("Unable to load your reviews. Please try again later.", ex);
            }
        }

        public async Task<CustomerReviewDto?> GetReviewByIdAsync(int reviewId, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{CustomerResource}/{reviewId}");
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerReviewDto>(_jsonOptions, cancellationToken);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve review {ReviewId}", reviewId);
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.2 — CREATE REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> CreateReviewAsync(int customerId, CreateReviewViewModel model, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    CustomerId = customerId,
                    ProductId = model.ProductId,
                    OrderId = model.OrderId,
                    OrderItemId = model.OrderItemId,
                    Rating = model.Rating,
                    Content = model.Content
                };

                var request = new HttpRequestMessage(HttpMethod.Post, CustomerResource)
                {
                    Content = JsonContent.Create(payload)
                };
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Gửi đánh giá thành công! Cảm ơn phản hồi của bạn.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return (false, "Chỉ khách hàng đã mua sản phẩm và hoàn tất đơn hàng mới có thể đánh giá.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return (false, "Bạn đã gửi đánh giá cho sản phẩm này từ trước.");
                }

                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(errorMsg) ? "Không thể gửi đánh giá. Vui lòng thử lại sau." : errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for customer {CustomerId}", customerId);
                return (false, "Lỗi kết nối máy chủ. Vui lòng thử lại sau.");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-07.3 — UPDATE REVIEWS (Customer)
        // ═══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> UpdateReviewAsync(int customerId, UpdateReviewViewModel model, string? jwtToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    CustomerId = customerId,
                    ReviewId = model.ReviewId,
                    Rating = model.Rating,
                    Content = model.Content
                };

                var request = new HttpRequestMessage(HttpMethod.Put, $"{CustomerResource}/{model.ReviewId}")
                {
                    Content = JsonContent.Create(payload)
                };
                AttachBearerToken(request, jwtToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Cập nhật đánh giá thành công.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Đánh giá không tồn tại hoặc đã bị xóa.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return (false, "Đánh giá này đã bị quản trị viên khóa hoặc bạn không có quyền sửa.");
                }

                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(errorMsg) ? "Không thể cập nhật đánh giá." : errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", model.ReviewId);
                return (false, "Lỗi kết nối máy chủ. Vui lòng thử lại sau.");
            }
        }
    }
}
