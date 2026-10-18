using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models;

public sealed class StaffModel
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class StaffCreateModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$", 
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;
}

public sealed class StaffUpdateModel
{
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }
}

public sealed class StaffIndexViewModel
{
    public List<StaffModel> Staffs { get; set; } = new();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public string Search { get; set; } = string.Empty;
    public string? StatusFilter { get; set; }
}

public sealed class StaffListRequest
{
    public string? SearchTerm { get; set; }
    public bool IncludeDelete { get; set; } = false;
    public bool IncludeDisable { get; set; } = true;
    public int? IgnoreStaffId { get; set; }
    public PaginationParams Pagination { get; set; } = new();
}

public sealed class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class StaffListResponse
{
    public int ActiveCount { get; set; }
    public PaginatedStaffs Pagination { get; set; } = new();
}

public sealed class PaginatedStaffs
{
    public List<StaffModel> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
