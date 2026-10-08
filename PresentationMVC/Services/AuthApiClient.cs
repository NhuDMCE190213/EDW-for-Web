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
            var request = new { Email = email, Password = password, RememberMe = rememberMe };
            var response = await _httpClient.PostAsJsonAsync("api/auth/customer/login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }

            return null;
        }

        public async Task<LoginResponse?> StaffLoginAsync(string email, string password, bool rememberMe)
        {
            var request = new { Email = email, Password = password, RememberMe = rememberMe };
            var response = await _httpClient.PostAsJsonAsync("api/auth/staff/login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }

            return null;
        }

        public async Task<(bool Success, string? ErrorMessage)> CustomerRegisterAsync(string fullName, string email, string phoneNumber, string password)
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
                return (false, "Lỗi từ máy chủ. Vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> VerifyEmailAsync(string email, string userType)
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

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string email, string password, string confirmPassword, string userType)
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
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
