using PresentationMVC.Models;

namespace PresentationMVC.Models.Cart
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int CustomerId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Quantity * (ProductVariant?.GetPrice() ?? 0);

        public CartItemProductVariantDto? ProductVariant { get; set; }
    }

    public class CartItemProductVariantDto
    {
        public Guid ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? Cpu { get; set; }
        public string? Ram { get; set; }
        public string? Storage { get; set; }
        public string? ScreenSize { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public Guid? PromotionId { get; set; }
        public PromotionModel? Promotion { get; set; }
        public bool IsOnSale => GetPrice() < Price;

        public decimal GetPrice()
        {
            if (Promotion is null || !Promotion.IsActive())
            {
                return Price;
            }

            if (Promotion.PromotionType == 0 && Promotion.Percentage.HasValue)
            {
                return Math.Max(0, Price * (1 - Promotion.Percentage.Value / 100m));
            }

            if (Promotion.PromotionType == 1 && Promotion.SalePrice.HasValue)
            {
                return Math.Max(0, Price - Promotion.SalePrice.Value);
            }

            return Price;
        }

        public string GetFormattedSpecs()
        {
            var specs = new List<string>();
            if (!string.IsNullOrWhiteSpace(Cpu)) specs.Add(Cpu);
            if (!string.IsNullOrWhiteSpace(Ram)) specs.Add($"{Ram} RAM");
            if (!string.IsNullOrWhiteSpace(Storage)) specs.Add(Storage);
            if (!string.IsNullOrWhiteSpace(ScreenSize)) specs.Add($"{ScreenSize} Display");
            return specs.Count > 0 ? string.Join(", ", specs) : "Standard Edition";
        }
    }

    public class CartItemCreateDto
    {
        public int CustomerId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartItemUpdateDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
