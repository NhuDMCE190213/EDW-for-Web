using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models.Review
{
    public class CustomerReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public Guid? OrderId { get; set; }
        public int? OrderItemId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Published"; // Published, Pending, Hidden, Flagged
        public bool IsLockedByAdmin { get; set; }
        public bool CanEdit => !IsLockedByAdmin && Status != "Hidden";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public Guid? OrderId { get; set; }

        public int? OrderItemId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá (1 - 5).")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Nội dung đánh giá phải từ 5 đến 1000 ký tự.")]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateReviewViewModel
    {
        [Required]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá (1 - 5).")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Nội dung đánh giá phải từ 5 đến 1000 ký tự.")]
        public string Content { get; set; } = string.Empty;

        public bool IsLocked { get; set; }
    }

    public class MyReviewsViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public List<CustomerReviewDto> Reviews { get; set; } = new();
        public int? FilterRating { get; set; }
        public string? FilterStatus { get; set; }
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}
