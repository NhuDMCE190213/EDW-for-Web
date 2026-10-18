namespace PresentationMVC.Models;

public sealed class ProductDetailViewModel
{
    public ProductCustomerModel Product { get; init; } = new();
    public IReadOnlyList<ProductVariantModel> Variants { get; init; } = [];
}
