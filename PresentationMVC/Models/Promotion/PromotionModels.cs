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

public class PromotionCreateModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public PromotionType PromotionType { get; set; }
    [Range(0, double.MaxValue)]
    public decimal? SalePrice { get; set; }
    [Range(0, double.MaxValue)]
    public decimal? ThresholdPrice { get; set; }
    [Range(0, 100)]
    public decimal? Percentage { get; set; }
    public bool IsReservedStock { get; set; } = true;
    [Range(0, int.MaxValue)]
    public int? MaxReservedStock { get; set; }
    public bool IsLimitedTime { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool IsDisable { get; set; }
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
