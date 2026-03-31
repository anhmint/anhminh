using System.ComponentModel.DataAnnotations;

namespace PoiApi.DTOs.Auth
{
    public class UpdateProfileDto
    {
        [Required]
        [RegularExpression(@"^[\w-\.]+@example\.com$", ErrorMessage = "Email phải có định dạng @example.com")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
