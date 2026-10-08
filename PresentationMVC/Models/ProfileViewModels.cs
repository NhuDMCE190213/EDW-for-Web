using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? Points { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsCustomer => Role.Equals("Customer", StringComparison.OrdinalIgnoreCase);
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class EditProfileViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsCustomer => Role.Equals("Customer", StringComparison.OrdinalIgnoreCase);

        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải từ 10-11 chữ số")]
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ cái, một số và một ký tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
