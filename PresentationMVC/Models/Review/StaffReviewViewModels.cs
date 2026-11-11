using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models.Review
{
    public class ReviewStaffDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public DateTime? FlaggedAt { get; set; }
        public string? FlaggedBy { get; set; }
        public bool IsHidden { get; set; }
        public string Status { get; set; } = "Published"; // Published, Flagged, Hidden, Pending
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class StaffReviewListRequest
    {
        public string? SearchTerm { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public string? Status { get; set; }
        public bool? OnlyFlagged { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class StaffReviewIndexViewModel
    {
        public List<ReviewStaffDto> Reviews { get; set; } = new();
        public int TotalReviews { get; set; }
        public int TotalFlagged { get; set; }
        public int TotalHidden { get; set; }
        public double AverageRating { get; set; }
        public StaffReviewListRequest Filter { get; set; } = new();
    }

    public class FlagReviewRequest
    {
        [Required]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do gắn cờ.")]
        [StringLength(250, ErrorMessage = "Lý do tối đa 250 ký tự.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ModerateReviewRequest
    {
        [Required]
        public int ReviewId { get; set; }

        public bool IsHidden { get; set; }

        public string? ModerationNote { get; set; }
    }
}
