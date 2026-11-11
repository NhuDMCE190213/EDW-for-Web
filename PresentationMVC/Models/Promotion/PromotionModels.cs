using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models.Promotion;

public enum PromotionType
{
    Percentage = 0,
    FixedAmount = 1
}

public sealed class PromotionDto
{
    public Guid PromotionId { get; set; }
    public string? Name { get; set; }
    public PromotionType PromotionType { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? ThresholdPrice { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsReservedStock { get; set; }
    public int? MaxReservedStock { get; set; }
    public bool IsLimitedTime { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDisabled { get; set; }
}

public class PromotionCreateModel : IValidatableObject
{
    [Required(ErrorMessage = "Promotion name is required.")]
    [StringLength(100, ErrorMessage = "Promotion name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
    public PromotionType PromotionType { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Sale Price must be greater than or equal to 0.")]
    public decimal? SalePrice { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Threshold Price must be greater than or equal to 0.")]
    public decimal? ThresholdPrice { get; set; }
    [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100.")]
    public decimal? Percentage { get; set; }
    public bool IsReservedStock { get; set; } = true;
    [Range(0, int.MaxValue, ErrorMessage = "Max reserved stock must be greater than or equal to 0.")]
    public int? MaxReservedStock { get; set; }
    public bool IsLimitedTime { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool IsDisable { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Promotion name cannot be empty.", new[] { nameof(Name) });
        }

        if (PromotionType == PromotionType.Percentage)
        {
            if (!Percentage.HasValue || Percentage.Value < 0 || Percentage.Value > 100)
            {
                yield return new ValidationResult("Percentage must be a valid number between 0 and 100.", new[] { nameof(Percentage) });
            }
        }
        else if (PromotionType == PromotionType.FixedAmount)
        {
            if (!SalePrice.HasValue || SalePrice.Value <= 0)
            {
                yield return new ValidationResult("Sale Price must be greater than 0.", new[] { nameof(SalePrice) });
            }

            if (!ThresholdPrice.HasValue || ThresholdPrice.Value <= 0)
            {
                yield return new ValidationResult("Threshold Price must be greater than 0.", new[] { nameof(ThresholdPrice) });
            }
            else if (SalePrice.HasValue && ThresholdPrice.Value <= SalePrice.Value)
            {
                yield return new ValidationResult("Threshold Price must be greater than Sale Price.", new[] { nameof(ThresholdPrice), nameof(SalePrice) });
            }
        }

        if (IsLimitedTime)
        {
            if (!StartAt.HasValue || !EndAt.HasValue)
            {
                yield return new ValidationResult("Please select both Start Date and End Date.", new[] { nameof(StartAt), nameof(EndAt) });
            }
            else if (EndAt.Value <= StartAt.Value)
            {
                yield return new ValidationResult("End Date must be after Start Date.", new[] { nameof(EndAt) });
            }
        }

        if (IsReservedStock && (!MaxReservedStock.HasValue || MaxReservedStock.Value < 0))
        {
            yield return new ValidationResult("Max reserved stock must be an integer greater than or equal to 0.", new[] { nameof(MaxReservedStock) });
        }
    }
}

public sealed class PromotionUpdateModel : PromotionCreateModel
{
}

public sealed class PromotionListRequest
{
    public string? SearchTerm { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeDisable { get; set; }
    public bool IncludeActive { get; set; } = true;
    public bool IncludeUpcoming { get; set; } = true;
    public bool IncludeExpired { get; set; } = true;
    public bool IncludeOutOfStock { get; set; } = true;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public PromotionType? DiscountType { get; set; }
    public decimal? MinDiscount { get; set; }
    public decimal? MaxDiscount { get; set; }
    public PaginationRequest Pagination { get; set; } = new();
}

public sealed class PaginationRequest
{
    private int _page = 1;
    private int _pageSize = 10;
    public int Page { get => _page; set => _page = value < 1 ? 1 : value; }
    public int PageSize { get => _pageSize; set => _pageSize = value < 1 ? 10 : Math.Min(value, 100); }
}

public sealed class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class PromotionListResponse
{
    public int ActiveCount { get; set; }
    public int Upcoming { get; set; }
    public int ExpiredOrDisabledCount { get; set; }
    public decimal MaxCost { get; set; }
    public PaginatedResponse<PromotionDto> Pagination { get; set; } = new();
}
