namespace LamillaEscudero.Application.Abstractions;

public interface ITextToSpeechService
{
    Task<string?> GenerateAudioBase64Async(
        string text,
        CancellationToken cancellationToken = default);
}
