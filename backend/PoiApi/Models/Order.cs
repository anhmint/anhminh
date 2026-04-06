using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoiApi.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Completed"; // Pending, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ShopId")]
        public Shop Shop { get; set; } = null!;
    }
}
