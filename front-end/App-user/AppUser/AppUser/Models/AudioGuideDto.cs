namespace AppUser.Models
{
    /// <summary>Audio guide stream URL for a POI</summary>
    public class AudioGuideDto
    {
        public int Id { get; set; }
        public int POIId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty; // Stream URL from server
        public string LanguageCode { get; set; } = "vi";
        public int DurationSeconds { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Formatted duration display (mm:ss)
        public string DurationDisplay =>
            $"{DurationSeconds / 60:D2}:{DurationSeconds % 60:D2}";
    }
}
