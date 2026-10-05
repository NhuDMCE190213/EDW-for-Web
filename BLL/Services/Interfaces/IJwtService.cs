using BLL.DTOs.Auth;

namespace BLL.Services.Interfaces
{
    /// <summary>
    /// Generates and validates JWT tokens for API authentication.
    /// Separated from business services so the token concern stays in BLL and
    /// the API project only references the interface, not Microsoft.IdentityModel directly.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Creates a signed JWT bearer token for a successfully authenticated user.
        /// </summary>
        /// <param name="userId">CustomerId or StaffId (PK in DB).</param>
        /// <param name="email">User's email address.</param>
        /// <param name="fullName">Display name (used as Name claim).</param>
        /// <param name="role">Role string stored in the RoleClaim.</param>
        /// <param name="rememberMe">
        ///   When true the token lifetime is extended (default 30 days);
        ///   when false it expires in 1 day.
        /// </param>
        /// <returns>A populated <see cref="LoginResponseDto"/> with the token and expiry.</returns>
        LoginResponseDto GenerateToken(int userId, string email, string fullName, string role, bool rememberMe = false);
    }
}
