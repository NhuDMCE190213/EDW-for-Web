namespace PresentationMVC.Models;

public sealed class CustomerStoreViewModel
{
    public IReadOnlyList<ProductStaffModel> Products { get; init; } = [];
    public IReadOnlyList<CategoryModel> Categories { get; init; } = [];
    public string Search { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}
