using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApiClient;

        public AuthController(AuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-01.1 – CUSTOMER LOGIN
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult CustomerLogin(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerLogin(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _authApiClient.CustomerLoginAsync(model.Email, model.Password, model.RememberMe);
            if (response == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            await SignInUserAsync(response, model.RememberMe);
            return RedirectAfterStaffLogin(response.Role.ToString(), returnUrl);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-01.2 – STAFF LOGIN
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult StaffLogin(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffLogin(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _authApiClient.StaffLoginAsync(model.Email, model.Password, model.RememberMe);
            if (response == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            await SignInUserAsync(response, model.RememberMe);
            return RedirectAfterStaffLogin(response.Role.ToString(), returnUrl);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-02 – LOGOUT
        // ═══════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("CustomerLogin", "Auth");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-03 – REGISTER (FOR CUSTOMER)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(null);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _authApiClient.CustomerRegisterAsync(
                model.FullName, model.Email, model.PhoneNumber, model.Password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng ký không thành công.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(CustomerLogin));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-04 – FORGOT PASSWORD (STEP 1: SEND OTP TO EMAIL)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ForgotPassword(string userType = "Customer")
        {
            var model = new ForgotPasswordViewModel { UserType = userType };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _authApiClient.SendForgotPasswordOtpAsync(model.Email, model.UserType);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Unable to send verification code to this email address.");
                return View(model);
            }

            TempData["InfoMessage"] = "A 6-digit OTP verification code has been sent to your email. Please check your inbox.";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email, userType = model.UserType });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-04 – FORGOT PASSWORD (STEP 2: VERIFY OTP)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult VerifyOtp(string email, string userType = "Customer")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new VerifyOtpViewModel
            {
                Email = email,
                UserType = userType
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, resetToken, errorMessage) = await _authApiClient.VerifyOtpAsync(model.Email, model.Otp, model.UserType);
            if (!success || string.IsNullOrWhiteSpace(resetToken))
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "OTP code is invalid or has expired.");
                return View(model);
            }

            TempData["SuccessMessage"] = "OTP verified successfully! Please enter your new password.";
            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, resetToken = resetToken, userType = model.UserType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email, string userType)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var (success, errorMessage) = await _authApiClient.SendForgotPasswordOtpAsync(email, userType);
            if (success)
            {
                TempData["InfoMessage"] = "A new OTP code has been sent to your email address.";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Unable to resend OTP code.";
            }

            return RedirectToAction(nameof(VerifyOtp), new { email = email, userType = userType });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-04 – RESET PASSWORD (STEP 3: SET NEW PASSWORD WITH RESET TOKEN)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ResetPassword(string email, string resetToken, string userType = "Customer")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(resetToken))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                ResetToken = resetToken,
                UserType = userType
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _authApiClient.ResetPasswordWithOtpAsync(
                model.Email, model.ResetToken, model.Password, model.UserType);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to reset password. The reset token may have expired.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password updated successfully! Please log in with your new password.";
            if (model.UserType.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(StaffLogin));
            }
            return RedirectToAction(nameof(CustomerLogin));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════════════
        private async Task SignInUserAsync(LoginResponse response, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim(ClaimTypes.Name, response.FullName),
                new Claim(ClaimTypes.Email, response.Email),
                new Claim(ClaimTypes.Role, response.Role.ToString()),
                new Claim("JWTToken", response.Token)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = response.ExpiresAt > DateTime.UtcNow ? response.ExpiresAt : DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(HomeController.Customer), "Home")!;
            }

            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(HomeController.Index), "Home")!
                : RedirectToAction(nameof(StaffProductController.Index), "StaffProduct")!;
        }

        private IActionResult RedirectAfterStaffLogin(string role, string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home")!;
            }

            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(HomeController.Customer), "Home")!;
            }

            return RedirectToAction(nameof(StaffProductController.Index), "StaffProduct")!;
        }
    }
}
