using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoiApi.Models
{
    /// <summary>User review for a shop after listening to audio</summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShopId { get; set; }

        public int? UserId { get; set; }

        public string? CustomerName { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ShopId")]
        public Shop Shop { get; set; } = null!;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
