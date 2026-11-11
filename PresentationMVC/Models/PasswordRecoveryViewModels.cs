using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Registered Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Account Type")]
        public string UserType { get; set; } = "Customer"; // "Customer" or "Staff"
    }

    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string UserType { get; set; } = "Customer";

        [Required(ErrorMessage = "Please enter the 6-digit OTP code")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be exactly 6 digits")]
        [Display(Name = "OTP Verification Code")]
        public string Otp { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string UserType { get; set; } = "Customer";

        [Required(ErrorMessage = "ResetToken is required")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your new password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$",
            ErrorMessage = "Password must contain letters, numbers, and special characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
