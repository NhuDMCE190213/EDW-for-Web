using DAL.Enums;

namespace BLL.DTOs.Auth
{
    /// <summary>
    /// Response payload returned by the API after a successful login.
    /// Contains the JWT bearer token and basic user information.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>JWT bearer token – attach as Authorization: Bearer {Token}</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Token expiry (UTC) so clients can schedule a refresh.</summary>
        public DateTime ExpiresAt { get; set; }

        // ── User info ──────────────────────────────────────────────────────────────

        /// <summary>Database PK of the authenticated user (CustomerId or StaffId).</summary>
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RoleEnum Role { get; set; }
    }
}
