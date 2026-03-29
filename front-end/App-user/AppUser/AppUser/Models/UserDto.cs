namespace AppUser.Models
{
    /// <summary>Data Transfer Object for User entity</summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "USER";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
