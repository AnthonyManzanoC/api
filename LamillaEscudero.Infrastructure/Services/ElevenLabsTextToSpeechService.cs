using System.Net.Http.Headers;
using System.Net.Http.Json;
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LamillaEscudero.Infrastructure.Services;

public sealed class ElevenLabsTextToSpeechService : ITextToSpeechService
{
    private const string DefaultVoiceId = "JBFqnCBsd6RMkjVDRZzb";
    private const string DefaultModelId = "eleven_multilingual_v2";

    private readonly HttpClient _httpClient;
    private readonly IOptions<ElevenLabsOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElevenLabsTextToSpeechService> _logger;

    public ElevenLabsTextToSpeechService(
        HttpClient httpClient,
        IOptions<ElevenLabsOptions> options,
        IConfiguration configuration,
        ILogger<ElevenLabsTextToSpeechService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GenerateAudioBase64Async(
        string text,
        CancellationToken cancellationToken = default)
    {
        var cleanText = text?.Trim();
        if (string.IsNullOrWhiteSpace(cleanText))
        {
            return null;
        }

        var settings = ResolveSettings();
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            _logger.LogWarning("No se puede sintetizar audio porque ElevenLabs no tiene una API Key configurada.");
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v1/text-to-speech/{Uri.EscapeDataString(settings.VoiceId)}?output_format=mp3_44100_128");

            request.Headers.TryAddWithoutValidation("xi-api-key", settings.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
            request.Content = JsonContent.Create(new
            {
                text = cleanText,
                model_id = settings.ModelId,
                language_code = "es",
                voice_settings = new
                {
                    stability = 0.45,
                    similarity_boost = 0.80,
                    style = 0.30,
                    use_speaker_boost = true
                }
            });

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "ElevenLabs devolvio el estado {StatusCode} al sintetizar audio del chat publico. Respuesta: {ResponseBody}",
                    (int)response.StatusCode,
                    Truncate(errorBody, 500));
                return null;
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return audioBytes.Length == 0
                ? null
                : Convert.ToBase64String(audioBytes);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("La llamada a ElevenLabs supero el tiempo de espera.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo sintetizar audio para el chat publico.");
            return null;
        }
    }

    private ResolvedElevenLabsSettings ResolveSettings()
    {
        var options = _options.Value;

        var apiKey = FirstNonEmpty(
            options.ApiKey,
            _configuration["ElevenLabs:ApiKey"],
            Environment.GetEnvironmentVariable("ElevenLabs__ApiKey"),
            Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY"));

        var voiceId = FirstNonEmpty(
            options.VoiceId,
            _configuration["ElevenLabs:VoiceId"],
            Environment.GetEnvironmentVariable("ElevenLabs__VoiceId"),
            Environment.GetEnvironmentVariable("ELEVENLABS_VOICE_ID"),
            DefaultVoiceId) ?? DefaultVoiceId;

        var modelId = FirstNonEmpty(
            options.ModelId,
            _configuration["ElevenLabs:ModelId"],
            Environment.GetEnvironmentVariable("ElevenLabs__ModelId"),
            Environment.GetEnvironmentVariable("ELEVENLABS_MODEL_ID"),
            DefaultModelId) ?? DefaultModelId;

        return new ResolvedElevenLabsSettings(apiKey, voiceId, modelId);
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static string Truncate(string? value, int maxLength)
    {
        var clean = (value ?? string.Empty).Trim();
        if (clean.Length <= maxLength)
        {
            return clean;
        }

        return clean[..maxLength].TrimEnd() + "...";
    }

    private sealed record ResolvedElevenLabsSettings(
        string? ApiKey,
        string VoiceId,
        string ModelId);
}
