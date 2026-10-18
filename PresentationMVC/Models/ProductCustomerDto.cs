namespace PresentationMVC.Models
{
    public class ProductCustomerModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ProductVariantModel> Variants { get; set; } = new();
        public List<Review.CustomerReviewDto> Reviews { get; set; } = new();
        public decimal MinPrice => Variants.Count > 0 ? Variants.Min(v => v.Price) : 0m;
        public decimal MaxPrice => Variants.Count > 0 ? Variants.Max(v => v.Price) : 0m;
        public int TotalStock => Variants.Sum(v => v.StockQuantity);
        public bool InStock => Variants.Any(v => v.StockQuantity > 0);
    }

    public class ProductCustomerCreateModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ProductCustomerUpdateModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ProductCustomerDeleteModel
    {
        public int ProductId { get; set; }
    }
}
