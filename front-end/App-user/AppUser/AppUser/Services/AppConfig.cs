namespace AppUser.Services
{
    public static class AppConfig
    {
        // Change this if your API is running on a different address
        // Android emulator must use 10.0.2.2 to reach host machine.
        public static string BaseApiUrl =
            DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5279/api/"
                : "http://localhost:5279/api/";
        
        // Helper to resolve relative media URLs
        public static string ResolveUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return string.Empty;
            if (relativeUrl.StartsWith("http")) return relativeUrl;
            
            var baseDomain = BaseApiUrl.Replace("/api/", "");
            return $"{baseDomain.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
    }
}
