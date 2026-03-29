using AppUser.Models;

namespace AppUser.Services
{
    /// <summary>
    /// POIService - Provides POI data.
    /// Currently mock data. TODO: Replace with HttpClient calls to API.
    /// API base: http://localhost:5279/api/
    /// </summary>
    public class POIService
    {
        // ─── MOCK DATA ────────────────────────────────────────────────────────
        private static readonly List<POIDto> MockPOIs = new()
        {
            new POIDto
            {
                Id = 1,
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=800",
                Location = "123 Lê Thánh Tôn, Quận 1, TP.HCM",
                Translations = new()
                {
                    new() { Id=1, POIId=1, LanguageCode="vi", Name="Quán Phở Hương Xưa", Description="Quán phở truyền thống với hơn 30 năm lịch sử tại Sài Gòn. Nước dùng được ninh từ xương bò trong 8 tiếng, tạo vị ngọt đậm đà khó quên. Phở bò tái nạm là món đặc trưng thu hút hàng trăm thực khách mỗi sáng." },
                    new() { Id=2, POIId=1, LanguageCode="en", Name="Huong Xua Pho Restaurant", Description="Traditional pho restaurant with over 30 years of history in Saigon. The broth is simmered from beef bones for 8 hours, creating an unforgettable rich flavor." }
                },
                Shop = new ShopDto
                {
                    Id = 1, Name = "Quán Phở Hương Xưa", IsActive = true,
                    Address = "123 Lê Thánh Tôn, Quận 1",
                    Description = "Phở truyền thống Sài Gòn",
                    Menus = new()
                    {
                        new MenuDto
                        {
                            Id=1, Name="Thực đơn chính", IsActive=true,
                            Items = new()
                            {
                                new() { Id=1, Name="Phở bò tái nạm", Price=65000, IsAvailable=true, Description="Phở bò tái + nạm đặc trưng" },
                                new() { Id=2, Name="Phở bò đặc biệt", Price=75000, IsAvailable=true, Description="Tái + gân + sách đầy đủ" },
                                new() { Id=3, Name="Phở gà", Price=55000, IsAvailable=true, Description="Phở gà ta nước trong" },
                                new() { Id=4, Name="Hủ tiếu bò kho", Price=60000, IsAvailable=true, Description="Hủ tiếu với bò kho đặc biệt" },
                            }
                        }
                    }
                },
                AudioGuides = new()
                {
                    new() { Id=1, POIId=1, Title="Giới thiệu Quán Phở Hương Xưa", LanguageCode="vi",
                        Description="Khám phá lịch sử và hương vị đặc trưng của quán phở nổi tiếng nhất Quận 1",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3", DurationSeconds=185 },
                    new() { Id=2, POIId=1, Title="Hương Xưa Pho Introduction", LanguageCode="en",
                        Description="Discover the history and unique flavors of District 1's most famous pho restaurant",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3", DurationSeconds=172 }
                }
            },
            new POIDto
            {
                Id = 2,
                ImageUrl = "https://images.unsplash.com/photo-1626804475297-41608ea09aeb?w=800",
                Location = "45 Nguyễn Huệ, Quận 1, TP.HCM",
                Translations = new()
                {
                    new() { Id=3, POIId=2, LanguageCode="vi", Name="Cơm Tấm Bà Tư", Description="Cơm tấm Nam Bộ chính gốc với sườn bì chả thơm lừng. Quán đã phục vụ người Sài Gòn từ năm 1985 với công thức gia truyền không thay đổi. Đây là điểm dừng chân không thể bỏ qua cho những ai yêu ẩm thực đường phố." },
                    new() { Id=4, POIId=2, LanguageCode="en", Name="Ba Tu Broken Rice", Description="Authentic Southern Vietnamese broken rice with fragrant grilled pork ribs. Serving Saigonese since 1985 with an unchanged family recipe." }
                },
                Shop = new ShopDto
                {
                    Id = 2, Name = "Cơm Tấm Bà Tư", IsActive = true,
                    Address = "45 Nguyễn Huệ, Quận 1",
                    Menus = new()
                    {
                        new MenuDto
                        {
                            Id=2, Name="Menu", IsActive=true,
                            Items = new()
                            {
                                new() { Id=5, Name="Cơm tấm sườn bì chả", Price=55000, IsAvailable=true },
                                new() { Id=6, Name="Cơm tấm sườn đặc biệt", Price=65000, IsAvailable=true },
                                new() { Id=7, Name="Cơm tấm bì chả", Price=45000, IsAvailable=true },
                                new() { Id=8, Name="Bì cuốn", Price=30000, IsAvailable=true },
                            }
                        }
                    }
                },
                AudioGuides = new()
                {
                    new() { Id=3, POIId=2, Title="Câu chuyện Cơm Tấm Bà Tư", LanguageCode="vi",
                        Description="Nghe kể về nguồn gốc và bí quyết làm nên hương vị cơm tấm đặc biệt",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3", DurationSeconds=210 }
                }
            },
            new POIDto
            {
                Id = 3,
                ImageUrl = "https://images.unsplash.com/photo-1569050467447-ce54b3bbc37d?w=800",
                Location = "78 Bùi Viện, Quận 1, TP.HCM",
                Translations = new()
                {
                    new() { Id=5, POIId=3, LanguageCode="vi", Name="Bánh Mì Huỳnh Hoa", Description="Bánh mì được mệnh danh là ngon nhất Sài Gòn. Ổ bánh căng bóng với nhân thịt, chả, pate và rau thơm tươi mát. Hàng quán lúc nào cũng đông khách từ sáng sớm đến tận chiều tối." },
                    new() { Id=6, POIId=3, LanguageCode="en", Name="Huynh Hoa Banh Mi", Description="Dubbed the best banh mi in Saigon. Crispy baguette filled with pork, pate, and fresh herbs. Always bustling from early morning to late evening." }
                },
                Shop = new ShopDto
                {
                    Id=3, Name="Bánh Mì Huỳnh Hoa", IsActive=true, Address="78 Bùi Viện, Quận 1",
                    Menus = new()
                    {
                        new MenuDto
                        {
                            Id=3, Name="Bánh mì", IsActive=true,
                            Items = new()
                            {
                                new() { Id=9, Name="Bánh mì thịt đặc biệt", Price=35000, IsAvailable=true },
                                new() { Id=10, Name="Bánh mì chả lụa", Price=25000, IsAvailable=true },
                                new() { Id=11, Name="Bánh mì trứng", Price=20000, IsAvailable=true },
                                new() { Id=12, Name="Bánh mì pate", Price=22000, IsAvailable=true },
                            }
                        }
                    }
                },
                AudioGuides = new()
                {
                    new() { Id=4, POIId=3, Title="Bí quyết bánh mì Huỳnh Hoa", LanguageCode="vi",
                        Description="Khám phá bí quyết làm nên chiếc bánh mì nổi tiếng nhất Sài Gòn",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3", DurationSeconds=156 }
                }
            },
            new POIDto
            {
                Id = 4,
                ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=800",
                Location = "10 Đặng Trần Côn, Quận 1, TP.HCM",
                Translations = new()
                {
                    new() { Id=7, POIId=4, LanguageCode="vi", Name="Chè Hiển Khánh", Description="Thiên đường chè Sài Gòn với hơn 50 loại chè khác nhau. Từ chè ba màu, chè thái, đến chè đậu đỏ bánh lọt — tất cả đều được nấu tươi mỗi ngày với nguyên liệu chọn lọc." },
                    new() { Id=8, POIId=4, LanguageCode="en", Name="Hien Khanh Sweet Soup", Description="Saigon's dessert paradise with over 50 types of Vietnamese sweet soups. All freshly made daily with carefully selected ingredients." }
                },
                Shop = new ShopDto
                {
                    Id=4, Name="Chè Hiển Khánh", IsActive=true, Address="10 Đặng Trần Côn, Quận 1",
                    Menus = new()
                    {
                        new MenuDto
                        {
                            Id=4, Name="Thực đơn chè", IsActive=true,
                            Items = new()
                            {
                                new() { Id=13, Name="Chè ba màu", Price=25000, IsAvailable=true },
                                new() { Id=14, Name="Chè thái", Price=30000, IsAvailable=true },
                                new() { Id=15, Name="Chè đậu đỏ bánh lọt", Price=22000, IsAvailable=true },
                                new() { Id=16, Name="Chè bưởi", Price=28000, IsAvailable=true },
                            }
                        }
                    }
                },
                AudioGuides = new()
                {
                    new() { Id=5, POIId=4, Title="Hành trình chè Sài Gòn", LanguageCode="vi",
                        Description="Tìm hiểu văn hóa ăn chè đặc sắc của người Sài Gòn qua câu chuyện của Hiển Khánh",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3", DurationSeconds=198 }
                }
            },
            new POIDto
            {
                Id = 5,
                ImageUrl = "https://images.unsplash.com/photo-1562802378-063ec186a863?w=800",
                Location = "Chợ Bến Thành, Quận 1, TP.HCM",
                Translations = new()
                {
                    new() { Id=9, POIId=5, LanguageCode="vi", Name="Bún Bò Huế Mệ Tư", Description="Bún bò Huế chuẩn vị miền Trung với nước dùng đặc trưng màu đỏ gạch từ sả, mắm ruốc và ớt. Thịt bò mềm, chả cua giòn — đây là điểm đến không thể bỏ qua cho người yêu món Huế." },
                    new() { Id=10, POIId=5, LanguageCode="en", Name="Me Tu Hue Beef Noodles", Description="Authentic Hue-style beef noodle soup with characteristic brick-red broth from lemongrass, shrimp paste, and chili. Tender beef and crispy crab rolls." }
                },
                Shop = new ShopDto
                {
                    Id=5, Name="Bún Bò Huế Mệ Tư", IsActive=true, Address="Chợ Bến Thành, Quận 1",
                    Menus = new()
                    {
                        new MenuDto
                        {
                            Id=5, Name="Menu", IsActive=true,
                            Items = new()
                            {
                                new() { Id=17, Name="Bún bò Huế đặc biệt", Price=70000, IsAvailable=true },
                                new() { Id=18, Name="Bún bò Huế thường", Price=55000, IsAvailable=true },
                                new() { Id=19, Name="Bún bò chả cua", Price=65000, IsAvailable=true },
                            }
                        }
                    }
                },
                AudioGuides = new()
                {
                    new() { Id=6, POIId=5, Title="Tinh hoa ẩm thực Huế", LanguageCode="vi",
                        Description="Nghe câu chuyện về hành trình mang ẩm thực cố đô Huế vào Sài Gòn của Mệ Tư",
                        AudioUrl="https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3", DurationSeconds=225 }
                }
            }
        };

        // ─── PUBLIC API ───────────────────────────────────────────────────────

        /// <summary>Get all POIs</summary>
        /// TODO: GET /api/pois
        public async Task<List<POIDto>> GetAllPOIsAsync()
        {
            await Task.Delay(300); // Simulate loading
            return MockPOIs.ToList();
        }

        /// <summary>Get featured POIs for home screen</summary>
        /// TODO: GET /api/pois/featured
        public async Task<List<POIDto>> GetFeaturedPOIsAsync(int count = 3)
        {
            await Task.Delay(200);
            return MockPOIs.Take(count).ToList();
        }

        /// <summary>Get POI by Id</summary>
        /// TODO: GET /api/pois/{id}
        public async Task<POIDto?> GetPOIByIdAsync(int id)
        {
            await Task.Delay(200);
            return MockPOIs.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>Search POIs by name or location</summary>
        /// TODO: GET /api/pois/search?q={query}
        public async Task<List<POIDto>> SearchPOIsAsync(string query, string lang = "vi")
        {
            await Task.Delay(200);
            if (string.IsNullOrWhiteSpace(query))
                return MockPOIs.ToList();

            var lower = query.ToLower();
            return MockPOIs.Where(p =>
                p.DisplayName(lang).ToLower().Contains(lower) ||
                (p.Location?.ToLower().Contains(lower) ?? false) ||
                (p.Shop?.Name.ToLower().Contains(lower) ?? false)
            ).ToList();
        }
    }
}
