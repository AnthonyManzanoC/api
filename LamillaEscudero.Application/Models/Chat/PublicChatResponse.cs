namespace LamillaEscudero.Application.Models.Chat;

public class PublicChatResponse
{
    public string Message { get; init; } = string.Empty;
    public string? SpeechText { get; init; }
    public string? AudioBase64 { get; init; }
    public bool IsHtml { get; init; }
    public bool StartLeadCapture { get; init; }
    public string SuggestionContext { get; init; } = "inicio";
}
