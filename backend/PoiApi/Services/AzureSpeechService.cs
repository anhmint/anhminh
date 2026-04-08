using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace PoiApi.Services;

public class AzureSpeechService
{
    private static readonly TimeSpan TtsTimeout = TimeSpan.FromSeconds(45);
    private readonly string _key;
    private readonly string _region;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AzureSpeechService> _logger;

    public AzureSpeechService(
        IConfiguration config,
        IWebHostEnvironment env,
        ILogger<AzureSpeechService> logger)
    {
        _key    = config["Azure:SpeechKey"]!;
        _region = config["Azure:SpeechRegion"]!;
        _env    = env;
        _logger = logger;
    }

    private static string GetVoiceName(string langCode) => langCode.ToLower() switch
    {
        "vi" => "vi-VN-HoaiMyNeural",
        "en" => "en-US-JennyNeural",
        "zh" => "zh-CN-XiaoxiaoNeural",
        _    => "en-US-JennyNeural" // Default to English for unknown
    };

    public async Task<string?> GenerateAudioAsync(int poiId, string langCode, string text)
    {
        try
        {
            var speechConfig = SpeechConfig.FromSubscription(_key, _region);
            speechConfig.SpeechSynthesisVoiceName = GetVoiceName(langCode);
            speechConfig.SetSpeechSynthesisOutputFormat(
                SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            if (!Directory.Exists(audioDir)) Directory.CreateDirectory(audioDir);

            // 1. Dọn dẹp: Tìm các file cũ của POI này và Ngôn ngữ này để xóa
            // Format cũ: poi_{id}_{lang}.mp3
            // Format mới: poi_{id}_{lang}_*.mp3
            var searchPattern = $"poi_{poiId}_{langCode}*.mp3";
            var existingFiles = Directory.GetFiles(audioDir, searchPattern);
            foreach (var file in existingFiles)
            {
                try { File.Delete(file); } catch { /* Ignore */ }
            }

            // 2. Tạo tên file duy nhất (Unique) để vượt qua cache trình duyệt triệt để
            var fileName = $"poi_{poiId}_{langCode}_{Guid.NewGuid().ToString("N")[..8]}.mp3";
            var filePath = Path.Combine(audioDir, fileName);

            // Do NOT use PullAudioOutputStream as its Read() method blocks synchronously and can hang forever.
            // By passing null for AudioConfig, the synthesized audio will be downloaded into result.AudioData asynchronously.
            using var synthesizer = new SpeechSynthesizer(speechConfig, null);

            var speakTask = synthesizer.SpeakTextAsync(text);
            var completed = await Task.WhenAny(speakTask, Task.Delay(TtsTimeout));
            if (completed != speakTask)
            {
                _logger.LogError("TTS timed out after {Timeout}s for poi {PoiId} lang {Lang}",
                    TtsTimeout.TotalSeconds, poiId, langCode);
                return null;
            }

            var result = await speakTask;

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                if (result.AudioData != null && result.AudioData.Length > 0)
                {
                    await File.WriteAllBytesAsync(filePath, result.AudioData);
                    _logger.LogInformation("Audio generated: {File} ({Size} bytes)", fileName, result.AudioData.Length);
                    return $"/audio/{fileName}";
                }
                else
                {
                    _logger.LogError("TTS succeeded but AudioData was empty.");
                    return null;
                }
            }

            if (result.Reason == ResultReason.Canceled)
            {
                var details = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogError("TTS canceled: {Reason} {ErrorCode} {ErrorDetails}",
                    details.Reason, details.ErrorCode, details.ErrorDetails);
            }
            else
            {
                _logger.LogError("TTS failed: {Reason}", result.Reason);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureSpeechService error");
            return null;
        }
    }
}