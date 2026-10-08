namespace PresentationMVC.Models.Promotion
{
    public class StaffPromotionIndexViewModel
    {
        public PromotionListResponse PromotionData { get; set; } = new();

        public string? SearchName { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public PromotionType? DiscountType { get; set; }
        public decimal? MinDiscount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MaxCost { get; set; } = 1000M;
        public bool IncludeDeleted { get; set; }
        public bool IncludeDisabled { get; set; }
        public bool IncludeActive { get; set; } = true;
        public bool IncludeUpcoming { get; set; } = true;
        public bool IncludeExpired { get; set; } = true;
        public bool IncludeOutOfStock { get; set; } = true;
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
