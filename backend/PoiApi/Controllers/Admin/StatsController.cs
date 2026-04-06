using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stats")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class AdminStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminStatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAdminStats()
        {
            var totalStores = _context.Shops.Count();
            var pendingStores = _context.Shops.Count(s => !s.IsActive);

            // Total customers that have role == USER
            var customerRole = _context.Roles.FirstOrDefault(r => r.Name == RoleConstants.User);
            var totalCustomers = customerRole != null ? _context.Users.Count(u => u.RoleId == customerRole.Id) : 0;

            var totalListens = _context.UsageHistories.Count();

            // Real revenue from Orders table
            var revenue = _context.Orders
                .Where(o => o.Status == "Completed")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            var topStores = _context.Shops
                .Select(s => new {
                    s.Name,
                    Category = "Chưa rõ",
                    Listens = 0
                })
                .Take(5)
                .ToList();

            var langStats = _context.Languages
                .Select(l => new {
                    Language = l.Name,
                    Listens = 0,
                    Percent = 0.0
                })
                .ToList();

            var monthlyOverview = new List<object>
            {
                new { Month = "Tháng hiện tại", NewStores = totalStores, NewCustomers = totalCustomers, TotalListens = totalListens, Revenue = revenue, Growth = 0.0 }
            };

            return Ok(new
            {
                TotalStores = totalStores,
                PendingStores = pendingStores,
                TotalCustomers = totalCustomers,
                TotalListens = totalListens,
                TotalRevenue = revenue,
                TopStores = topStores,
                LangStats = langStats,
                MonthlyOverview = monthlyOverview
            });
        }

        /// <summary>
        /// Revenue statistics filtered by month and/or year.
        /// GET /api/admin/stats/revenue?year=2026 → full year grouped by month
        /// GET /api/admin/stats/revenue?month=4&year=2026 → single month grouped by shop
        /// </summary>
        [HttpGet("revenue")]
        public IActionResult GetRevenueStats([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentYear = year ?? DateTime.UtcNow.Year;

            var ordersQuery = _context.Orders
                .Include(o => o.Shop)
                .Where(o => o.Status == "Completed")
                .Where(o => o.CreatedAt.Year == currentYear);

            if (month.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt.Month == month.Value);
            }

            // Total revenue
            var totalRevenue = ordersQuery.Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Revenue by shop
            var revenueByShop = ordersQuery
                .GroupBy(o => new { o.ShopId, o.Shop.Name })
                .Select(g => new
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.Name,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Revenue by month (when filtering by year only)
            var revenueByMonth = ordersQuery
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return Ok(new
            {
                Year = currentYear,
                Month = month,
                TotalRevenue = totalRevenue,
                RevenueByShop = revenueByShop,
                RevenueByMonth = revenueByMonth
            });
        }
    }
}
