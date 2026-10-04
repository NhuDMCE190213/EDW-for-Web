using System.Security.Claims;
using BLL.DTOs.Customer;
using BLL.DTOs.Staff;
using BLL.Services.Interfaces;
using DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles user profile operations for authenticated actors (Customer, Staff, Admin):
    /// - UC-05.1: View Profile
    /// - UC-05.2: Edit Profile
    /// - UC-05.3: Change Password
    /// </summary>
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IStaffService _staffService;

        public ProfileController(ICustomerService customerService, IStaffService staffService)
        {
            _customerService = customerService;
            _staffService = staffService;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // UC-05.1 – View Profile (All Actors)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Retrieves the current authenticated user's profile information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User identity could not be verified." });

            if (role == RoleEnum.Customer.ToString())
            {
                var customer = await _customerService.GetByEmailAsync(email);
                if (customer == null)
                    return NotFound(new { message = "Customer profile not found." });

                var profile = new CustomerProfileDto
                {
                    CustomerId = customer.CustomerId,
                    Email = customer.Email,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    Points = customer.Points
                };
                return Ok(profile);
            }
            else // Staff or Admin
            {
                var staff = await _staffService.GetByEmailAsync(email);
                if (staff == null)
                    return NotFound(new { message = "Staff profile not found." });

                var profile = new StaffProfileDto
                {
                    StaffId = staff.StaffId,
                    Email = staff.Email,
                    FullName = staff.FullName,
                    IsActive = staff.IsActive,
                    CreatedAt = staff.CreatedAt,
                    UpdatedAt = staff.UpdatedAt
                };
                return Ok(profile);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // UC-05.2 – Edit Profile (All Actors)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Updates the current authenticated Customer's profile.
        /// </summary>
        [HttpPut("customer")]
        public async Task<IActionResult> UpdateCustomerProfile([FromBody] CustomerProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User identity could not be verified." });

            var customer = await _customerService.GetByEmailAsync(email);
            if (customer == null)
                return NotFound(new { message = "Customer profile not found." });

            dto.Email = customer.Email;
            dto.Points = customer.Points;

            var success = await _customerService.UpdateProfileAsync(customer.CustomerId, dto);
            if (!success)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update profile." });

            return Ok(new { message = "Profile updated successfully.", profile = dto });
        }

        /// <summary>
        /// Updates the current authenticated Staff's profile.
        /// </summary>
        [HttpPut("staff")]
        public async Task<IActionResult> UpdateStaffProfile([FromBody] StaffProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User identity could not be verified." });

            var staff = await _staffService.GetByEmailAsync(email);
            if (staff == null)
                return NotFound(new { message = "Staff profile not found." });

            dto.StaffId = staff.StaffId;
            dto.Email = staff.Email;
            dto.IsActive = staff.IsActive;

            var success = await _staffService.UpdateProfileAsync(staff.StaffId, dto);
            if (!success)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update profile." });

            return Ok(new { message = "Profile updated successfully.", profile = dto });
        }

        // ══════════════════════════════════════════════════════════════════════════
        // UC-05.3 – Change Password (All Actors)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Changes the password for the current authenticated user.
        /// </summary>
        [HttpPut("change-password")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] CustomerChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User identity could not be verified." });

            if (role == RoleEnum.Customer.ToString())
            {
                var customer = await _customerService.GetByEmailAsync(email);
                if (customer == null)
                    return NotFound(new { message = "Customer not found." });

                var success = await _customerService.SetPasswordAsync(customer.CustomerId, dto.NewPassword);
                if (!success)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update password." });

                return Ok(new { message = "Password changed successfully." });
            }
            else // Staff or Admin
            {
                var staff = await _staffService.GetByEmailAsync(email);
                if (staff == null)
                    return NotFound(new { message = "Staff not found." });

                var success = await _staffService.SetPasswordAsync(staff.StaffId, dto.NewPassword);
                if (!success)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update password." });

                return Ok(new { message = "Password changed successfully." });
            }
        }
    }
}
