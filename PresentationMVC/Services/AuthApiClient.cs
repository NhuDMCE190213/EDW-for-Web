using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PresentationMVC.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public async Task<LoginResponse?> CustomerLoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var request = new { Email = email, Password = password, RememberMe = rememberMe };
                var response = await _httpClient.PostAsJsonAsync("api/auth/customer/login", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<LoginResponse?> StaffLoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var request = new { Email = email, Password = password, RememberMe = rememberMe };
                var response = await _httpClient.PostAsJsonAsync("api/auth/staff/login", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CustomerRegisterAsync(string fullName, string email, string phoneNumber, string password)
        {
            try
            {
                var request = new { FullName = fullName, Email = email, PhoneNumber = phoneNumber, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/customer/register", request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, error?.Message ?? "Đăng ký không thành công. Vui lòng thử lại.");
                }
                catch
                {
                    return (false, "Lỗi từ máy chủ API. Vui lòng thử lại.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Không thể kết nối đến máy chủ Backend API được cấu hình.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> SendForgotPasswordOtpAsync(string email, string userType)
        {
            try
            {
                var endpoint = userType.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                    ? "api/auth/staff/forgot-password"
                    : "api/auth/customer/forgot-password";

                var response = await _httpClient.PostAsJsonAsync(endpoint, new { Email = email });
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, error?.Message ?? "Unable to send OTP code to this email address.");
                }
                catch
                {
                    return (false, "Invalid email address or server did not respond.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Unable to connect to Backend API server.");
            }
            catch (Exception ex)
            {
                return (false, $"System error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ResetToken, string? ErrorMessage)> VerifyOtpAsync(string email, string otp, string userType)
        {
            try
            {
                var endpoint = userType.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                    ? "api/auth/staff/verify-otp"
                    : "api/auth/customer/verify-otp";

                var response = await _httpClient.PostAsJsonAsync(endpoint, new { Email = email, Otp = otp });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyOtpResultDto>(_jsonOptions);
                    return (true, result?.ResetToken, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, null, error?.Message ?? "Invalid OTP verification code.");
                }
                catch
                {
                    return (false, null, "Server error while verifying OTP code.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, null, "Unable to connect to Backend API server.");
            }
            catch (Exception ex)
            {
                return (false, null, $"System error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordWithOtpAsync(string email, string resetToken, string newPassword, string userType)
        {
            try
            {
                var endpoint = userType.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                    ? "api/auth/staff/reset-password-otp"
                    : "api/auth/customer/reset-password-otp";

                var request = new { Email = email, ResetToken = resetToken, NewPassword = newPassword };
                var response = await _httpClient.PostAsJsonAsync(endpoint, request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, error?.Message ?? "Failed to update password.");
                }
                catch
                {
                    return (false, "Server connection error while updating password.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Unable to connect to Backend API server.");
            }
            catch (Exception ex)
            {
                return (false, $"System error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> VerifyEmailAsync(string email, string userType)
        {
            try
            {
                var endpoint = userType.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                    ? "api/auth/staff/verify-email"
                    : "api/auth/customer/verify-email";

                var response = await _httpClient.PostAsJsonAsync(endpoint, new { Email = email });
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, error?.Message ?? "Email không tồn tại trong hệ thống.");
                }
                catch
                {
                    return (false, "Email không hợp lệ hoặc máy chủ không phản hồi.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Không thể kết nối đến máy chủ Backend API được cấu hình.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string email, string password, string confirmPassword, string userType)
        {
            try
            {
                var endpoint = userType.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                    ? "api/auth/staff/reset-password"
                    : "api/auth/customer/reset-password";

                var request = new { Email = email, Password = password, ConfirmPassword = confirmPassword };
                var response = await _httpClient.PostAsJsonAsync(endpoint, request);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                    return (false, error?.Message ?? "Cập nhật mật khẩu không thành công.");
                }
                catch
                {
                    return (false, "Lỗi kết nối máy chủ khi đổi mật khẩu.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Không thể kết nối đến máy chủ Backend API được cấu hình.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }

    public class VerifyOtpResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public LoginRole Role { get; set; }
    }

    public enum LoginRole
    {
        Admin,
        Staff,
        Customer
    }
}
