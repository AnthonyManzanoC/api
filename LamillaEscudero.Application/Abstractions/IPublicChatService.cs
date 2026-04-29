using LamillaEscudero.Application.Models.Chat;

namespace LamillaEscudero.Application.Abstractions;

public interface IPublicChatService
{
    Task<PublicChatResponse> GetReplyAsync(
        string userMessage,
        CancellationToken cancellationToken = default);

    Task<string?> GenerateSpeechAudioBase64Async(
        string text,
        CancellationToken cancellationToken = default);
}
