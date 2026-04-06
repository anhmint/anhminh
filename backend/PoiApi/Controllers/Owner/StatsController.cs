using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;
using System.Globalization;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/stats/seller")]
    [Authorize(Roles = "ADMIN,OWNER")]
    public class SellerStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SellerStatsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Basic seller stats (kept for backward compatibility).
        /// </summary>
        [HttpGet("{sellerId}")]
        public IActionResult GetSellerStats(int sellerId, [FromQuery] int? storeId = null)
        {
            var query = _context.Shops.Where(s => s.OwnerId == sellerId);
            if (storeId.HasValue && storeId.Value > 0)
            {
                query = query.Where(s => s.Id == storeId.Value);
            }

            var shops = query.ToList();
            var shopIds = shops.Select(s => s.Id).ToList();

            if (!shopIds.Any())
                return Ok(new List<object>());

            var totalListens = shops.Sum(s => s.ListenCount);
            var totalViews = shops.Sum(s => s.ViewCount);
            var totalReviews = _context.Reviews.Count(r => shopIds.Contains(r.ShopId));

            return Ok(new List<object>
            {
                new { 
                    TotalListens = totalListens, 
                    TotalViews = totalViews, 
                    TotalReviews = totalReviews, 
                    Month = "Tất cả thời gian" 
                }
            });
        }

        /// <summary>
        /// Get recent reviews for a specific store.
        /// GET /api/stores/{storeId}/reviews
        /// </summary>
        [HttpGet("/api/stores/{storeId}/reviews")]
        public IActionResult GetStoreReviews(int storeId, [FromQuery] int? sellerId = null)
        {
            var query = _context.Reviews.AsQueryable();
            if (storeId > 0)
            {
                query = query.Where(r => r.ShopId == storeId);
            }
            else if (sellerId.HasValue && sellerId.Value > 0)
            {
                var shopIds = _context.Shops.Where(s => s.OwnerId == sellerId.Value).Select(s => s.Id).ToList();
                query = query.Where(r => shopIds.Contains(r.ShopId));
            }
            else
            {
                return Ok(new List<object>());
            }

            var reviews = query
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new
                {
                    r.Id,
                    r.ShopId,
                    r.CustomerName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToList();

            return Ok(reviews);
        }

        /// <summary>
        /// Revenue statistics for a seller, filtered by week.
        /// GET /api/stats/seller/{sellerId}/revenue?week=2026-04-06
        /// The 'week' param is any date within the desired week (Monday-Sunday).
        /// If omitted, defaults to the current week.
        /// </summary>
        [HttpGet("{sellerId}/revenue")]
        public IActionResult GetSellerRevenue(int sellerId, [FromQuery] string? week, [FromQuery] int? storeId = null)
        {
            // Determine the target week
            DateTime targetDate;
            if (!string.IsNullOrEmpty(week) && DateTime.TryParse(week, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                targetDate = parsed;
            }
            else
            {
                targetDate = DateTime.UtcNow;
            }

            // Calculate Monday and Sunday of the target week
            var dayOfWeek = targetDate.DayOfWeek;
            int daysToMonday = dayOfWeek == DayOfWeek.Sunday ? -6 : -(int)dayOfWeek + 1;
            var weekStart = targetDate.Date.AddDays(daysToMonday); // Monday 00:00
            var weekEnd = weekStart.AddDays(7); // Next Monday 00:00

            // Get shop IDs owned by this seller
            var q = _context.Shops.Where(s => s.OwnerId == sellerId);
            if (storeId.HasValue && storeId.Value > 0)
            {
                q = q.Where(s => s.Id == storeId.Value);
            }
            var shopIds = q.Select(s => s.Id).ToList();

            if (!shopIds.Any())
            {
                return Ok(new
                {
                    WeekStart = weekStart.ToString("yyyy-MM-dd"),
                    WeekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
                    TotalRevenue = 0m,
                    DailyRevenue = new List<object>(),
                    ShopRevenue = new List<object>()
                });
            }

            var ordersInWeek = _context.Orders
                .Include(o => o.Shop)
                .Where(o => shopIds.Contains(o.ShopId))
                .Where(o => o.Status == "Completed")
                .Where(o => o.CreatedAt >= weekStart && o.CreatedAt < weekEnd)
                .ToList();

            var totalRevenue = ordersInWeek.Sum(o => o.TotalAmount);

            // Daily revenue (Mon-Sun)
            var vietnameseDayNames = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            var dailyRevenue = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = weekStart.AddDays(i);
                    var dayOrders = ordersInWeek.Where(o => o.CreatedAt.Date == day.Date);
                    return new
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        DayName = vietnameseDayNames[i],
                        Revenue = dayOrders.Sum(o => o.TotalAmount),
                        OrderCount = dayOrders.Count()
                    };
                })
                .ToList();

            // Revenue by shop
            var shopRevenue = ordersInWeek
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

            return Ok(new
            {
                WeekStart = weekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
                TotalRevenue = totalRevenue,
                DailyRevenue = dailyRevenue,
                ShopRevenue = shopRevenue
            });
        }
    }
}
