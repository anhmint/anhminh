namespace AppUser.Models
{
    /// <summary>Menu DTO</summary>
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemDto> Items { get; set; } = new();
    }

    /// <summary>Menu Item DTO</summary>
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }

        // Helper: Formatted price in VND
        public string PriceDisplay => $"{Price:N0} đ";
    }
}
