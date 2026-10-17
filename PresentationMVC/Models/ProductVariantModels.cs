using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PresentationMVC.Models;

public sealed class ProductVariantModel
{
    public Guid ProductVariantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? ScreenSize { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductId { get; set; }
    public Guid? PromotionId { get; set; }
    public PromotionModel? Promotion { get; set; }

    [JsonIgnore]
    public decimal EffectiveDiscountedPrice
    {
        get
        {
            if (DiscountedPrice.HasValue && DiscountedPrice.Value > 0 && DiscountedPrice.Value < Price && Promotion != null && Promotion.IsActive())
            {
                return DiscountedPrice.Value;
            }

            if (Promotion != null && Promotion.IsActive())
            {
                if (Promotion.PromotionType == 0 && Promotion.Percentage.HasValue) // Percentage
                {
                    return Math.Max(0, Price * (1 - Promotion.Percentage.Value / 100m));
                }
                else if (Promotion.PromotionType == 1 && Promotion.SalePrice.HasValue) // FixedAmount
                {
                    return Math.Max(0, Price - Promotion.SalePrice.Value);
                }
            }

            return Price;
        }
    }

    [JsonIgnore]
    public bool HasActiveDiscount => Promotion != null && Promotion.IsActive() && EffectiveDiscountedPrice < Price;

    [JsonIgnore]
    public decimal DisplayPrice => EffectiveDiscountedPrice;
}

public sealed class PromotionModel
{
    public Guid PromotionId { get; set; }
    public string? Name { get; set; }
    public int PromotionType { get; set; } // 0: Percentage, 1: FixedAmount
    public decimal? SalePrice { get; set; }
    public decimal? ThresholdPrice { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsReservedStock { get; set; }
    public int? MaxReservedStock { get; set; }
    public bool IsLimitedTime { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDisabled { get; set; }

    public bool IsActive()
    {
        if (IsDeleted || IsDisabled)
        {
            return false;
        }
        var now = DateTime.UtcNow;
        if (IsLimitedTime)
        {
            if (EndAt.HasValue && EndAt.Value < now) return false;
            if (StartAt.HasValue && StartAt.Value > now) return false;
        }
        if (IsReservedStock && MaxReservedStock.HasValue && MaxReservedStock.Value <= 0)
        {
            return false;
        }
        return true;
    }
}

public class ProductVariantCreateModel
{
    [Required]
    public string Color { get; set; } = string.Empty;
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? ScreenSize { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductId { get; set; }
    public Guid? PromotionId { get; set; }
}

public sealed class ProductVariantUpdateModel : ProductVariantCreateModel
{
    public Guid ProductVariantId { get; set; }
}

public sealed class ProductVariantMatrixModel
{
    public int ProductId { get; set; }
    public List<VariantMatrixItemModel> Variants { get; set; } = new();
}

public sealed class VariantMatrixItemModel
{
    [Required]
    public string Color { get; set; } = string.Empty;
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? ScreenSize { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
