namespace PoiApi.DTOs.Auth
{
    /// <summary>DTO for user-app registration (role is always USER)</summary>
    public class UserRegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
    }
}
