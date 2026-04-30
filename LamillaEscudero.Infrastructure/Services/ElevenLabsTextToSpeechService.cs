using System.Net.Http.Headers;
using System.Net.Http.Json;
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Chat;
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

    public async Task<TextToSpeechResult> GenerateAudioBase64Async(
        string text,
        CancellationToken cancellationToken = default)
    {
        var cleanText = text?.Trim();
        if (string.IsNullOrWhiteSpace(cleanText))
        {
            return TextToSpeechResult.Empty;
        }

        var settings = ResolveSettings();
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            const string errorMessage = "No se encontro configuracion para ElevenLabs:ApiKey.";
            _logger.LogWarning("No se puede sintetizar audio porque ElevenLabs no tiene una API Key configurada.");
            return TextToSpeechResult.Failure(errorMessage);
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
                var errorMessage = BuildHttpErrorMessage(response, errorBody);
                _logger.LogWarning(
                    "ElevenLabs devolvio el estado {StatusCode} al sintetizar audio del chat publico. Respuesta: {ResponseBody}",
                    (int)response.StatusCode,
                    Truncate(errorBody, 500));
                return TextToSpeechResult.Failure(errorMessage);
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (audioBytes.Length == 0)
            {
                const string errorMessage = "ElevenLabs devolvio el audio vacio.";
                _logger.LogWarning("ElevenLabs devolvio un audio vacio para el chat publico.");
                return TextToSpeechResult.Failure(errorMessage);
            }

            return TextToSpeechResult.Success(Convert.ToBase64String(audioBytes));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            const string errorMessage = "La llamada a ElevenLabs supero el tiempo de espera.";
            _logger.LogWarning(errorMessage);
            return TextToSpeechResult.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo sintetizar audio para el chat publico.");
            return TextToSpeechResult.Failure(ex.Message);
        }
    }

    private ResolvedElevenLabsSettings ResolveSettings()
    {
        var options = _options.Value;
        var section = _configuration.GetSection(ElevenLabsOptions.SectionName);

        var apiKey = FirstNonEmpty(
            section[nameof(ElevenLabsOptions.ApiKey)],
            options.ApiKey);

        var voiceId = FirstNonEmpty(
            section[nameof(ElevenLabsOptions.VoiceId)],
            options.VoiceId,
            DefaultVoiceId) ?? DefaultVoiceId;

        var modelId = FirstNonEmpty(
            section[nameof(ElevenLabsOptions.ModelId)],
            options.ModelId,
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

    private static string BuildHttpErrorMessage(HttpResponseMessage response, string? errorBody)
    {
        var statusCode = (int)response.StatusCode;
        var reasonPhrase = response.ReasonPhrase?.Trim();
        var statusText = string.IsNullOrWhiteSpace(reasonPhrase)
            ? statusCode.ToString()
            : $"{statusCode} {reasonPhrase}";

        var details = Truncate(errorBody, 300);
        return string.IsNullOrWhiteSpace(details)
            ? statusText
            : $"{statusText} - {details}";
    }

    private sealed record ResolvedElevenLabsSettings(
        string? ApiKey,
        string VoiceId,
        string ModelId);
}
