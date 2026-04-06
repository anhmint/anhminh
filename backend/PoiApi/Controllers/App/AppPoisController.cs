using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PoiApi.Data;
using PoiApi.DTOs.App;
using Microsoft.EntityFrameworkCore;
using PoiApi.Services;
using PoiApi.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/app/pois")]
public class AppPoisController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly AzureTranslationService _translator;
    private readonly IConfiguration _configuration;

    public AppPoisController(
        AppDbContext context,
        IMapper mapper,
        AzureTranslationService translator,
        IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _translator = translator;
        _configuration = configuration;
    }

    // 🔹 LIST (Home screen)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
    {
        // 1. Chỉ lấy những gian hàng đang hoạt động
        var shops = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Where(s => s.IsActive && s.Poi != null)
            .ToListAsync();

        var resultList = new List<AppPoiListDto>();
        var namesToTranslate = new List<string>();
        var shopsNeedTranslation = new List<Tuple<int, string>>();

        foreach (var s in shops)
        {
            var p = s.Poi!;
            var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang);
            
            if (t != null)
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = t.AudioUrl ?? p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = t.Name
                });
            }
            else if (lang == "vi")
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = s.Name
                });
            }
            else
            {
                // Cần dịch tự động
                shopsNeedTranslation.Add(new Tuple<int, string>(p.Id, s.Name));
                namesToTranslate.Add(s.Name);
                
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = s.Name // Tạm thời để tên gốc, sẽ update sau batch translate
                });
            }
        }

        // BATCH TRANSLATE if needed
        if (lang != "vi" && namesToTranslate.Any())
        {
            var translatedNames = await _translator.TranslateListAsync(namesToTranslate, lang);
            if (translatedNames != null && translatedNames.Count == namesToTranslate.Count)
            {
                for (int i = 0; i < shopsNeedTranslation.Count; i++)
                {
                    var id = shopsNeedTranslation[i].Item1;
                    var item = resultList.FirstOrDefault(x => x.Id == id);
                    if (item != null) item.Name = translatedNames[i];
                }
            }
        }

        return Ok(resultList);
    }

    // 🔹 DETAIL (POI detail screen)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(
        int id,
        [FromQuery] string lang = "vi")
    {
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Include(s => s.Menus)
                .ThenInclude(m => m.MenuItems)
            .FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);

        if (shop == null || shop.Poi == null) return NotFound();

        var p = shop.Poi;
        POITranslation t;
        var requestedTrans = p.Translations.FirstOrDefault(x => x.LanguageCode == lang);
        var viTrans = p.Translations.FirstOrDefault(x => x.LanguageCode == "vi");
        string sourceName = viTrans?.Name ?? shop.Name;
        string sourceDesc = viTrans?.Description ?? shop.Description ?? "";

        // Nếu đã có bản dịch nhưng tên vẫn giống hệt tiếng Việt (có thể do lỗi dịch trước đó)
        // và ngôn ngữ yêu cầu không phải là tiếng Việt, thì thử dịch lại on-the-fly.
        if (requestedTrans != null && lang != "vi" && requestedTrans.Name == sourceName)
        {
            t = new POITranslation
            {
                LanguageCode = lang,
                Name = await _translator.TranslateAsync(sourceName, lang) ?? sourceName,
                Description = await _translator.TranslateAsync(sourceDesc, lang) ?? sourceDesc,
                AudioUrl = requestedTrans.AudioUrl // Giữ lại AudioUrl cũ
            };
        }
        else if (requestedTrans != null)
        {
            t = requestedTrans;
        }
        else
        {
            // Missing translation -> Auto-translate
            if (lang == "vi")
            {
                t = new POITranslation { Name = sourceName, Description = sourceDesc, LanguageCode = "vi" };
            }
            else
            {
                t = new POITranslation
                {
                    LanguageCode = lang,
                    Name = await _translator.TranslateAsync(sourceName, lang) ?? sourceName,
                    Description = await _translator.TranslateAsync(sourceDesc, lang) ?? sourceDesc,
                    AudioUrl = ""
                };
            }
        }

        var dto = new AppPoiDetailDto
        {
            Id = p.Id,
            ImageUrl = p.ImageUrl ?? "",
            Location = p.Location ?? "",
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            AudioUrl = t.AudioUrl
                ?? viTrans?.AudioUrl
                ?? p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl
                ?? "",
            Name = t.Name,
            Description = t.Description ?? "",
            Menus = shop.Menus?.Select(m => new AppMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Items = m.MenuItems.Select(i => new AppMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price
                }).ToList()
            }).ToList() ?? new(),
            AvailableLanguages = new[] { "vi", "en", "zh" }.Select(code =>
            {
                var tr = p.Translations.FirstOrDefault(x => x.LanguageCode == code);
                return new AppLanguageDto
                {
                    Code = code,
                    Name = GetLanguageName(code),
                    HasAudio = tr != null && !string.IsNullOrEmpty(tr.AudioUrl)
                };
            }).ToList()
        };

        // AUTO-TRANSLATE Menu and Items if not Vietnamese
        if (lang != "vi" && dto.Menus.Any())
        {
            var textsToTranslate = new List<string>();
            foreach (var menu in dto.Menus)
            {
                textsToTranslate.Add(menu.Name);
                foreach (var item in menu.Items)
                {
                    textsToTranslate.Add(item.Name);
                }
            }

            var translatedTexts = await _translator.TranslateListAsync(textsToTranslate, lang);
            if (translatedTexts != null && translatedTexts.Count == textsToTranslate.Count)
            {
                int index = 0;
                foreach (var menu in dto.Menus)
                {
                    menu.Name = translatedTexts[index++];
                    foreach (var item in menu.Items)
                    {
                        item.Name = translatedTexts[index++];
                    }
                }
            }
        }

        return Ok(dto);
    }

    // 🔹 TRACK VIEW
    [HttpPost("{id}/view")]
    public async Task<IActionResult> TrackView(int id)
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null) return NotFound();

        shop.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // 🔹 TRACK LISTEN & REVENUE
    [HttpPost("{id}/listen")]
    public async Task<IActionResult> TrackListen(int id, [FromQuery] string deviceId = "anonymous")
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null) return NotFound();

        shop.ListenCount++;
        shop.ViewCount++;

        var revenuePerListen = _configuration.GetValue<decimal?>("BusinessRules:RevenuePerListenVnd") ?? 20000m;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usageDeviceId = !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : deviceId;

        var order = new Order
        {
            ShopId = shop.Id,
            TotalAmount = revenuePerListen,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        };
        _context.Orders.Add(order);

        var usage = new UsageHistory
        {
            DeviceId = usageDeviceId,
            ShopId = shop.Id,
            ListenedAt = DateTime.UtcNow,
            DurationSeconds = 0
        };
        _context.UsageHistories.Add(usage);

        await _context.SaveChangesAsync();

        return Ok(new { success = true, shop.ListenCount });
    }

    // 🔹 SUBMIT REVIEW
    [Authorize]
    [HttpPost("{id}/reviews")]
    public async Task<IActionResult> SubmitReview(int id, [FromBody] AppReviewDto dto)
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null) return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để gửi đánh giá." });
        }

        var hasListened = await _context.UsageHistories
            .AnyAsync(x => x.ShopId == shop.Id && x.DeviceId == $"user:{userId}");
        if (!hasListened)
        {
            return StatusCode(403, new { success = false, message = "Bạn cần nghe POI trước khi gửi đánh giá." });
        }

        var review = new Review
        {
            ShopId = shop.Id,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CustomerName = dto.CustomerName ?? "Khách hàng",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private string GetLanguageName(string code) => code.ToLower() switch
    {
        "vi" => "Tiếng Việt",
        "en" => "English",
        "zh" => "中文 (Chinese)",
        _ => code.ToUpper()
    };
}
