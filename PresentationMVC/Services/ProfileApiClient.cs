using PresentationMVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PresentationMVC.Services
{
    public class ProfileApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProfileApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<UserProfileViewModel?> GetProfileAsync(string jwtToken, string role)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            var isCustomer = role.Equals("Customer", StringComparison.OrdinalIgnoreCase);

            var profile = new UserProfileViewModel
            {
                Role = role
            };

            if (isCustomer)
            {
                if (jsonElement.TryGetProperty("customerId", out var idProp)) profile.UserId = idProp.GetInt32();
                if (jsonElement.TryGetProperty("email", out var emailProp)) profile.Email = emailProp.GetString() ?? "";
                if (jsonElement.TryGetProperty("fullName", out var nameProp)) profile.FullName = nameProp.GetString() ?? "";
                if (jsonElement.TryGetProperty("phoneNumber", out var phoneProp)) profile.PhoneNumber = phoneProp.GetString();
                if (jsonElement.TryGetProperty("points", out var ptsProp)) profile.Points = ptsProp.GetInt32();
            }
            else
            {
                if (jsonElement.TryGetProperty("staffId", out var idProp)) profile.UserId = idProp.GetInt32();
                if (jsonElement.TryGetProperty("email", out var emailProp)) profile.Email = emailProp.GetString() ?? "";
                if (jsonElement.TryGetProperty("fullName", out var nameProp)) profile.FullName = nameProp.GetString() ?? "";
                if (jsonElement.TryGetProperty("isActive", out var activeProp)) profile.IsActive = activeProp.GetBoolean();
                if (jsonElement.TryGetProperty("createdAt", out var crProp)) profile.CreatedAt = crProp.GetDateTime();
            }

            return profile;
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(string jwtToken, string fullName, string? phoneNumber)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "api/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = JsonContent.Create(new
            {
                FullName = fullName,
                PhoneNumber = phoneNumber ?? string.Empty
            });

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                return (false, error?.Message ?? "Cập nhật hồ sơ thất bại.");
            }
            catch
            {
                return (false, "Lỗi từ máy chủ khi cập nhật thông tin.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string jwtToken, string newPassword, string confirmPassword)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "api/profile/password");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = JsonContent.Create(new
            {
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            });

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                return (false, error?.Message ?? "Đổi mật khẩu thất bại.");
            }
            catch
            {
                return (false, "Lỗi từ máy chủ khi đổi mật khẩu.");
            }
        }
    }
}
