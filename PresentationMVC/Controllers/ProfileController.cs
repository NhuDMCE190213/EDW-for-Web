using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models;
using PresentationMVC.Services;
using System.Security.Claims;

namespace PresentationMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ProfileApiClient _profileApiClient;

        public ProfileController(ProfileApiClient profileApiClient)
        {
            _profileApiClient = profileApiClient;
        }

        private string? GetJwtToken() => User.FindFirst("JWTToken")?.Value;
        private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer";

        // ═══════════════════════════════════════════════════════════════════════
        // UC-05.1 – VIEW PROFILE
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            var role = GetUserRole();
            var profile = await _profileApiClient.GetProfileAsync(token, role);

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Không thể tải thông tin hồ sơ người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("CustomerLogin", "Auth");
            }

            return View(profile);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-05.2 – EDIT PROFILE
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var token = GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            var role = GetUserRole();
            var profile = await _profileApiClient.GetProfileAsync(token, role);

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Không thể tải hồ sơ.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditProfileViewModel
            {
                UserId = profile.UserId,
                Email = profile.Email,
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Role = profile.Role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var token = GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _profileApiClient.UpdateProfileAsync(
                token, model.FullName, model.PhoneNumber);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Cập nhật hồ sơ thất bại.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Hồ sơ của bạn đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UC-05.3 – CHANGE PASSWORD
        // ═══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var token = GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("CustomerLogin", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _profileApiClient.ChangePasswordAsync(
                token, model.NewPassword, model.ConfirmPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đổi mật khẩu thất bại.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
