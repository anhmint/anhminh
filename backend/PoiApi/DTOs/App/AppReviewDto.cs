using System.ComponentModel.DataAnnotations;

namespace PoiApi.DTOs.App
{
    public class AppReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
        
        public string? CustomerName { get; set; }
    }
}
