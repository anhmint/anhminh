using System.Net.Http.Json;
using System.Text.Json;
using AppUser.Models;

namespace AppUser.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private UserDto? _currentUser;
        private string? _token;

        public bool IsLoggedIn => _currentUser != null;
        public UserDto? CurrentUser => _currentUser;
        public string? Token => _token;

        // Tự động phân giải localhost cho Android Emulator
        public static string BaseAddress =
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5279" : "http://localhost:5279";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseAddress);
        }

        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Email = email.Trim(), Password = password });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        _token = result.Token;
                        _currentUser = new UserDto
                        {
                            Email = email,
                            Role = result.Role,
                            IsActive = true
                        };

                        // Bạn có thể lưu Token vào SecureStorage ở đây nếu muốn giữ trạng thái đăng nhập
                        // await SecureStorage.Default.SetAsync("auth_token", _token);

                        return (true, "Đăng nhập thành công");
                    }
                }
                
                return (false, "Email hoặc mật khẩu không đúng.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName)
        {
            try
            {
                var payload = new
                {
                    Email = email.Trim(),
                    Password = password,
                    FullName = fullName.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("/api/auth/register-user", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đăng ký thành công.");
                }
                
                // Đọc thông báo lỗi từ backend
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "Đăng ký thất bại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            _token = null;
            // SecureStorage.Default.Remove("auth_token");
            return Task.CompletedTask;
        }

        public UserDto? GetCurrentUser() => _currentUser;

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
