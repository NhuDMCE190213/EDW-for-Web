using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models;

public sealed class CustomerModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CustomerCreateModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Phone number must be a valid 10-digit Vietnamese phone number starting with 0 (e.g. 0912345678).")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$", 
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Points must be between 0 and 1,000,000.")]
    [Display(Name = "Reward Points")]
    public int Points { get; set; } = 0;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;
}

public sealed class CustomerUpdateModel
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Phone number must be a valid 10-digit Vietnamese phone number starting with 0 (e.g. 0912345678).")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Points must be between 0 and 1,000,000.")]
    [Display(Name = "Reward Points")]
    public int Points { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }
}

public sealed class CustomerIndexViewModel
{
    public List<CustomerModel> Customers { get; set; } = new();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public string Search { get; set; } = string.Empty;
    public string? StatusFilter { get; set; }
}

public sealed class CustomerListRequest
{
    public string? SearchTerm { get; set; }
    public bool IncludeDelete { get; set; } = false;
    public bool IncludeDisable { get; set; } = true;
    public PaginationParams Pagination { get; set; } = new();
}

public sealed class CustomerListResponse
{
    public int ActiveCount { get; set; }
    public PaginatedCustomers Pagination { get; set; } = new();
}

public sealed class PaginatedCustomers
{
    public List<CustomerModel> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
