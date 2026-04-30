using LamillaEscudero.Application.Models.Chat;

namespace LamillaEscudero.Application.Abstractions;

public interface ITextToSpeechService
{
    Task<TextToSpeechResult> GenerateAudioBase64Async(
        string text,
        CancellationToken cancellationToken = default);
}
