namespace PresentationMVC.Models.Product.Customer
{
    public class ProductCustomerModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;


        public string? ImageUrl { get; set; }

        //public IEnumerable<ProductVariantCustomerDto> ProductVariants { get; set; } = new List<ProductVariantCustomerDto>();

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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
