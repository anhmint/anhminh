using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.Models;

public class PoiService : IPoiService
{
	private readonly AppDbContext _context;

	public PoiService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<POIAdminDto>> GetAllAsync(string lang)
	{
		var pois = await _context.POIs
			.Include(p => p.Translations)
			.ToListAsync();

		return pois.Select(p =>
		{
			var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang)
					?? p.Translations.First();

			return new POIAdminDto
			{
				Id = p.Id,
				ImageUrl = p.ImageUrl,
				Location = p.Location,
				Latitude = p.Latitude,
				Longitude = p.Longitude,
				Name = t.Name,
				Description = t.Description,
				AudioUrl = t.AudioUrl
			};
		}).ToList();
	}

	public async Task<POIAdminDto?> GetByIdAsync(int id, string lang)
	{
		var poi = await _context.POIs
			.Include(p => p.Translations)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (poi == null) return null;

		var t = poi.Translations.First(x => x.LanguageCode == lang);

		return new POIAdminDto
		{
			Id = poi.Id,
			ImageUrl = poi.ImageUrl,
			Location = poi.Location,
			Latitude = poi.Latitude,
			Longitude = poi.Longitude,
			Name = t.Name,
			Description = t.Description,
			AudioUrl = t.AudioUrl
		};
	}

	public async Task<POIAdminDto> CreateAsync(CreatePoiDto dto)
	{
		var poi = new POI
		{
			ImageUrl = dto.ImageUrl,
			Location = dto.Location,
			Latitude = dto.Latitude,
			Longitude = dto.Longitude,
			Translations = dto.Translations.Select(t => new POITranslation
			{
				LanguageCode = t.LanguageCode,
				Name = t.Name,
				Description = t.Description
			}).ToList()
		};

		_context.POIs.Add(poi);
		await _context.SaveChangesAsync();

		var first = poi.Translations.First();

		return new POIAdminDto
		{
			Id = poi.Id,
			ImageUrl = poi.ImageUrl,
			Location = poi.Location,
			Latitude = poi.Latitude,
			Longitude = poi.Longitude,
			Name = first.Name,
			Description = first.Description
		};
	}

	public async Task<POIAdminDto?> UpdateAsync(int id, CreatePoiDto dto)
	{
		var poi = await _context.POIs
			.Include(p => p.Translations)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (poi == null) return null;

		poi.ImageUrl = dto.ImageUrl;
		poi.Location = dto.Location;
		poi.Latitude = dto.Latitude;
		poi.Longitude = dto.Longitude;

		// Sync translations
		foreach (var tDto in dto.Translations)
		{
			var existing = poi.Translations.FirstOrDefault(x => x.LanguageCode == tDto.LanguageCode);
			if (existing != null)
			{
				existing.Name = tDto.Name;
				existing.Description = tDto.Description;
			}
			else
			{
				poi.Translations.Add(new POITranslation
				{
					LanguageCode = tDto.LanguageCode,
					Name = tDto.Name,
					Description = tDto.Description
				});
			}
		}

		await _context.SaveChangesAsync();

		var first = poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi") ?? poi.Translations.First();

		return new POIAdminDto
		{
			Id = poi.Id,
			ImageUrl = poi.ImageUrl,
			Location = poi.Location,
			Name = first.Name,
			Description = first.Description,
			AudioUrl = first.AudioUrl
		};
	}

	public async Task<bool> LinkShopsAsync(int poiId, List<int> shopIds)
	{
		// Reset old links
		var oldShops = await _context.Shops.Where(s => s.PoiId == poiId).ToListAsync();
		foreach (var s in oldShops) s.PoiId = null;

		// Set new links
		var newShops = await _context.Shops.Where(s => shopIds.Contains(s.Id)).ToListAsync();
		foreach (var s in newShops) s.PoiId = poiId;

		await _context.SaveChangesAsync();
		return true;
	}
}
