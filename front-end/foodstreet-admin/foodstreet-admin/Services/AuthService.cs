using foodstreet_admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;

namespace foodstreet_admin.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly TokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PendingLoginService _pending;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApiService api,
        TokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        PendingLoginService pending,
        ILogger<AuthService> logger)
    {
        _api = api;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _pending = pending;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, string RedirectUrl)> LoginAsync(LoginRequest request)
    {
        try
        {
            var result = await _api.PostAsync<LoginRequest, LoginResponse>(
                "auth/login",
                request);

            if (result == null)
                return (false, "Đã xảy ra lỗi khi đăng nhập!", "");

            _tokenService.SetToken(result.Token);

            var role = result.Role?.ToUpper() switch
            {
                "ADMIN" => "ADMIN",
                "OWNER" => "OWNER",
                _ => result.Role?.ToUpper() ?? "USER"
            };

            // ---- ĐÃ SỬA: Dùng PendingLoginService thay vì HttpClient, lưu cả JwtToken ----
            var token = _pending.Store(result.UserId, request.Email, role, request.RememberMe, result.Token);

            // ---- ĐÃ SỬA: Trỏ về trạm /auth/finalize trong Program.cs ----
            string redirect = $"/auth/finalize?t={token}";

            return (true, "Đăng nhập thành công!", redirect);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Login API call failed");
            try 
            {
                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Trim().StartsWith("{"))
                {
                    var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ex.Message, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (error != null && error.ContainsKey("message"))
                        return (false, error["message"], "");
                }
            }
            catch { }
            return (false, "Email hoặc mật khẩu không đúng!", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync unexpected error");
            return (false, "Đã xảy ra lỗi. Vui lòng thử lại!", "");
        }
    }
    

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return (false, "Mật khẩu xác nhận không khớp!");
        if (request.Password.Length < 6)
            return (false, "Mật khẩu phải ít nhất 6 ký tự!");

        try
        {
            var payload = new
            {
                email = request.Email,
                password = request.Password,
                role = string.IsNullOrEmpty(request.Role) ? "OWNER" : request.Role
            };

            await _api.PostAsync<object, object>("auth/register", payload);
            return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            return (false, "Email đã được sử dụng hoặc vai trò không hợp lệ!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterAsync failed");
            return (false, "Không thể kết nối đến server. Vui lòng thử lại!");
        }
    }

    public async Task LogoutAsync()
    {
        _tokenService.ClearToken();
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<(bool Success, UserModel? User, string Message)> GetMeAsync()
    {
        try
        {
            var me = await _api.GetAsync<MeResponse>("auth/me");
            if (me == null)
                return (false, null, "Không thể tải thông tin tài khoản.");

            var user = new UserModel
            {
                Id = me.Id,
                Email = me.Email ?? "",
                FullName = me.FullName ?? "",
                Role = me.Role ?? "",
                Status = me.IsActive ? "Active" : "Disabled",
                CreatedAt = me.CreatedAt
            };

            return (true, user, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMeAsync failed");
            return (false, null, "Lỗi khi tải thông tin tài khoản.");
        }
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, string fullName, string email)
    {
        try
        {
            var payload = new { email, fullName };
            await _api.PutAsync<object, object>("auth/profile", payload);
            return (true, "Lưu thông tin thành công!");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            return (false, "Email đã được sử dụng hoặc không hợp lệ!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync failed");
            return (false, "Lỗi server khi lưu thông tin.");
        }
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPwd, string newPwd)
    {
        try
        {
            var payload = new { currentPassword = currentPwd, newPassword = newPwd };
            await _api.PutAsync<object, object>("auth/profile", payload);
            return (true, "Đổi mật khẩu thành công!");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            return (false, "Mật khẩu hiện tại không đúng!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePasswordAsync failed");
            return (false, "Lỗi server khi đổi mật khẩu.");
        }
    }
}

internal class MeResponse
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class PendingLoginService
{
    private readonly Dictionary<string, (int UserId, string Email, string Role, bool RememberMe, string JwtToken, DateTime Expires)> _pending = new();

    public string Store(int userId, string email, string role, bool rememberMe, string jwtToken)
    {
        var token = Guid.NewGuid().ToString("N");
        _pending[token] = (userId, email, role, rememberMe, jwtToken, DateTime.UtcNow.AddMinutes(2));
        return token;
    }

    public (int UserId, string Email, string Role, bool RememberMe, string JwtToken)? Consume(string token)
    {
        if (_pending.TryGetValue(token, out var entry) && entry.Expires > DateTime.UtcNow)
        {
            _pending.Remove(token);
            return (entry.UserId, entry.Email, entry.Role, entry.RememberMe, entry.JwtToken);
        }
        return null;
    }
}