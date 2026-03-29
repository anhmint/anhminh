namespace AppUser.Models
{
    /// <summary>Shop (cửa hàng) DTO</summary>
    public class ShopDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? PoiId { get; set; }
        public bool IsActive { get; set; }
        public List<MenuDto> Menus { get; set; } = new();
    }
}
