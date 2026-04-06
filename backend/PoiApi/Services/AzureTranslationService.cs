using System.Text;
using System.Text.Json;

namespace PoiApi.Services;

public class AzureTranslationService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureTranslationService> _logger;

    public AzureTranslationService(
        IConfiguration config, 
        HttpClient httpClient, 
        ILogger<AzureTranslationService> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
    }

    private string MapLanguageCode(string code) => code.ToLower() switch
    {
        "zh" => "zh-Hans",
        _ => code
    };

    public async Task<string?> TranslateAsync(string text, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text) || targetLang == "vi") return text;
        var results = await TranslateListAsync(new List<string> { text }, targetLang);
        return results?.FirstOrDefault();
    }

    public async Task<List<string>?> TranslateListAsync(List<string> texts, string targetLang)
    {
        if (texts == null || !texts.Any() || targetLang == "vi") return texts;

        // Try primary translator key first
        var key = _config["Azure:TranslatorKey"];
        var region = _config["Azure:TranslatorRegion"] ?? "global";
        
        var result = await PerformTranslationAsync(texts, targetLang, key, region);
        
        // If result is null (indicates auth failure or other non-recoverable error but we returned null to signal retry)
        if (result == null)
        {
            // Fallback to SpeechKey
            _logger.LogWarning("Azure Translator failed. Retrying with SpeechKey...");
            var speechKey = _config["Azure:SpeechKey"];
            var speechRegion = _config["Azure:SpeechRegion"] ?? "global";
            
            if (speechKey != key)
            {
                result = await PerformTranslationAsync(texts, targetLang, speechKey, speechRegion);
            }
        }

        return result ?? texts; // Return texts as fallback if all failed
    }

    private async Task<List<string>?> PerformTranslationAsync(List<string> texts, string targetLang, string? key, string region)
    {
        if (string.IsNullOrEmpty(key)) return null;

        try
        {
            var mappedLang = MapLanguageCode(targetLang);
            var endpoint = _config["Azure:TranslatorEndpoint"] ?? "https://api.cognitive.microsofttranslator.com/";
            var url = $"{endpoint.TrimEnd('/')}/translate?api-version=3.0&from=vi&to={mappedLang}";
            
            var requestBody = texts.Select(t => new { Text = t }).ToArray();
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            
            if (!string.Equals(region, "global", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);
            }

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Translator API error ({Code}): {Error}", response.StatusCode, errorJson);
                
                // If it's an auth error, return null to signal retry
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return null;
                }
                return null; // For now retry on any error
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            
            var results = new List<string>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var translations = item.GetProperty("translations");
                if (translations.GetArrayLength() > 0)
                {
                    results.Add(translations[0].GetProperty("text").GetString() ?? "");
                }
                else
                {
                    results.Add("");
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureTranslationService PerformTranslationAsync failure for {Lang}", targetLang);
            return null;
        }
    }
}
