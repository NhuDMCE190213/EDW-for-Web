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
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductId { get; set; }
    public Guid? PromotionId { get; set; }
    public PromotionModel? Promotion { get; set; }

    [JsonIgnore]
    public decimal DisplayPrice => Promotion is null ? Price : Price;
}

public sealed class PromotionModel
{
    public string? Name { get; set; }
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
