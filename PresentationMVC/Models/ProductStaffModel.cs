using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models
{
    public class ProductStaffModel
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;


        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductStaffCreateModel
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class ProductStaffUpdateModel
    {
        public int ProductId { get; set; }
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class AdminProductIndexViewModel
    {
        public IReadOnlyList<ProductStaffModel> Products { get; init; } = [];
        public IReadOnlyList<CategoryModel> Categories { get; init; } = [];
        public ProductStaffUpdateModel Form { get; init; } = new();
        public bool IsEditing { get; init; }
        public string Search { get; init; } = string.Empty;
        public int CategoryFilter { get; init; }
    }

    public class AdminDashboardViewModel
    {
        public IReadOnlyList<ProductStaffModel> Products { get; init; } = [];
        public IReadOnlyList<CategoryModel> Categories { get; init; } = [];
    }

    public class ProductStaffDeleteModel
    {
        public int ProductId { get; set; }
    }
}
