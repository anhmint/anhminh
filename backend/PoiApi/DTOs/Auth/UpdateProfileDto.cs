using System.ComponentModel.DataAnnotations;

namespace PoiApi.DTOs.Auth
{
    public class UpdateProfileDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
