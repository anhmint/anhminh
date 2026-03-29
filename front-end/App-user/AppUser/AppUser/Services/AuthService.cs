using AppUser.Models;

namespace AppUser.Services
{
    /// <summary>
    /// AuthService - Handles user authentication.
    /// Currently uses mock data. Replace with HttpClient calls when API is ready.
    /// </summary>
    public class AuthService
    {
        private UserDto? _currentUser;
        private string? _token;

        public bool IsLoggedIn => _currentUser != null;
        public UserDto? CurrentUser => _currentUser;
        public string? Token => _token;

        // Mock user database
        private static readonly List<(string Email, string Password, UserDto User)> MockUsers = new()
        {
            ("user@foodtour.vn", "123456", new UserDto
            {
                Id = 1,
                Email = "user@foodtour.vn",
                Role = "USER",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30)
            }),
            ("nguyen.van.a@gmail.com", "password", new UserDto
            {
                Id = 2,
                Email = "nguyen.van.a@gmail.com",
                Role = "USER",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-10)
            })
        };

        /// <summary>Login with email and password</summary>
        public Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            // Simulate network delay
            // TODO: Replace with: POST /api/auth/login
            var match = MockUsers.FirstOrDefault(u =>
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)
                && u.Password == password);

            if (match.User == null)
                return Task.FromResult((false, "Email hoặc mật khẩu không đúng."));

            _currentUser = match.User;
            _token = $"mock-token-{Guid.NewGuid():N}";

            return Task.FromResult((true, "Đăng nhập thành công"));
        }

        /// <summary>Logout current user</summary>
        public Task LogoutAsync()
        {
            _currentUser = null;
            _token = null;
            return Task.CompletedTask;
        }

        /// <summary>Get current logged-in user</summary>
        public UserDto? GetCurrentUser() => _currentUser;
    }
}
