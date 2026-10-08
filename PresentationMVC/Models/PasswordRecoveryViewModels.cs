using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Loại tài khoản")]
        public string UserType { get; set; } = "Customer"; // "Customer" or "Staff"
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string UserType { get; set; } = "Customer";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$",
            ErrorMessage = "Mật khẩu phải chứa chữ cái, số và ký tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
