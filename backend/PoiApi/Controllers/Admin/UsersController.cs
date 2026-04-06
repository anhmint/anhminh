using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        // Chỉ ADMIN mới được xem tất cả user
        [HttpGet]
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {   
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.Name,
                    Status = u.IsActive ? "Active" : "Disabled",
                    RegisteredAt = u.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/admin/users/sellers
        [HttpGet("sellers")]
        public IActionResult GetSellers()
        {
            var sellers = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == RoleConstants.Owner)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Phone = "", // Tạm thời để trống vì User model chưa có field Phone
                    Status = u.IsActive ? "Active" : "Disabled",
                    RegisteredAt = u.CreatedAt,
                    StoreCount = _context.Shops.Count(s => s.OwnerId == u.Id)
                })
                .ToList();

            return Ok(sellers);
        }

        // GET: api/admin/users/customers
        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            var customers = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == RoleConstants.User)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Phone = "", // Placeholder
                    Status = u.IsActive ? "Active" : "Disabled",
                    RegisteredAt = u.CreatedAt,
                    TotalListens = 0 // Future extension with UsageHistory
                })
                .ToList();

            return Ok(customers);
        }

        // PATCH: api/admin/users/{id}/status
        [HttpPatch("{id}/status")]
        public IActionResult UpdateSellerStatus(int id, [FromQuery] string status)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound("User not found");

            user.IsActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Shops)
                    .ThenInclude(s => s.Poi)
                        .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound("User not found");

            if (user.Role != null && user.Role.Name == RoleConstants.Admin)
            {
                return StatusCode(403, "Không được phép xóa tài khoản Admin.");
            }

            // If user is an owner, remove his shops and their POI/POI translations first.
            // This avoids FK/relationship issues (User -> Shop is Restrict on delete).
            var shopsToDelete = user.Shops?.ToList() ?? new List<Shop>();
            foreach (var shop in shopsToDelete)
            {
                if (shop.Poi != null)
                {
                    // Remove POI translations explicitly before removing POI.
                    _context.POITranslations.RemoveRange(shop.Poi.Translations);
                    _context.POIs.Remove(shop.Poi);
                }

                _context.Shops.Remove(shop);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tài khoản thành công" });
        }

        // GET: api/users/me
        // Lấy thông tin user đang đăng nhập
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == int.Parse(userId));

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Email,
                Role = user.Role.Name
            });
        }

        [HttpPost("create-owner")]
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult CreateOwner(CreateOwnerDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            var ownerRole = _context.Roles
                .FirstOrDefault(r => r.Name == RoleConstants.Owner);

            if (ownerRole == null)
                return StatusCode(500, "Owner role not found");

            var owner = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = ownerRole.Id
            };

            _context.Users.Add(owner);
            _context.SaveChanges();

            // (bước sau) tạo POI cho owner
            // var poi = new POI { Name = dto.ShopName, OwnerId = owner.Id }

            return Ok("Owner created successfully");
        }

        [HttpPost("seed-customers")]
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult SeedCustomers()
        {
            var customerRole = _context.Roles.FirstOrDefault(r => r.Name == RoleConstants.User);
            if (customerRole == null) return StatusCode(500, "Customer role not found");

            var existingCount = _context.Users.Count(u => u.RoleId == customerRole.Id);
            if (existingCount >= 5) return Ok("Dữ liệu khách hàng đã có sẵn.");

            var customers = new List<User>
            {
                new User { FullName = "Nguyễn Văn A", Email = "customer.a@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = customerRole.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new User { FullName = "Trần Thị B", Email = "customer.b@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = customerRole.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new User { FullName = "Lê Hoàng C", Email = "customer.c@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = customerRole.Id, IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new User { FullName = "Phạm Minh D", Email = "customer.d@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = customerRole.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };

            _context.Users.AddRange(customers);
            _context.SaveChanges();

            return Ok("Đã tạo dữ liệu khách hàng mẫu thành công.");
        }
    }
}
