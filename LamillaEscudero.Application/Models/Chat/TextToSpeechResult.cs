namespace LamillaEscudero.Application.Models.Chat;

public sealed record TextToSpeechResult(
    string? AudioBase64,
    string? ErrorMessage)
{
    public static TextToSpeechResult Success(string audioBase64)
        => new(audioBase64, null);

    public static TextToSpeechResult Failure(string errorMessage)
        => new(null, errorMessage);

    public static TextToSpeechResult Empty { get; } = new(null, null);
}
