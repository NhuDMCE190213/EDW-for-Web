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
            return RedirectToLocal(returnUrl);
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
            return RedirectToLocal(returnUrl);
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
                return RedirectToAction(nameof(HomeController.Index), "Home");
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
        // UC-04 – FORGOT PASSWORD (STEP 1: VERIFY EMAIL)
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

            var (success, errorMessage) = await _authApiClient.VerifyEmailAsync(model.Email, model.UserType);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, userType = model.UserType });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-04 – RESET PASSWORD (STEP 2: SET NEW PASSWORD)
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ResetPassword(string email, string userType = "Customer")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
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

            var (success, errorMessage) = await _authApiClient.ResetPasswordAsync(
                model.Email, model.Password, model.ConfirmPassword, model.UserType);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đặt lại mật khẩu thất bại.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật thành công! Vui lòng đăng nhập với mật khẩu mới.";
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
                new Claim(ClaimTypes.Role, response.Role),
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
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
