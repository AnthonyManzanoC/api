using LamillaEscudero.Application.Models.Chat;

namespace LamillaEscudero.Application.Abstractions;

public interface IPublicChatService
{
    Task<PublicChatResponse> GetReplyAsync(
        string userMessage,
        CancellationToken cancellationToken = default);
}
