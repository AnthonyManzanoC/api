using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LamillaEscudero.Application.Abstractions;
using LamillaEscudero.Application.Models.Chat;
using LamillaEscudero.Application.Models.Miembros;
using LamillaEscudero.Application.Models.Servicios;
using Microsoft.Extensions.Logging;

namespace LamillaEscudero.Infrastructure.Services;

public sealed class PublicChatService : IPublicChatService
{
    private static readonly string[] SensitiveKeywords =
    {
        "admin", "administrador", "panel", "dashboard", "backoffice",
        "password", "contrasena", "clave", "token", "jwt",
        "smtp", "api key", "apikey", "secret", "secreto",
        "base de datos", "database", "connection string", "cadena de conexion",
        "ruta interna", "/admin", "hangfire", "stack trace", "exception", "servidor"
    };

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "hola", "buenos", "buenas", "saludos", "quisiera", "quiero", "necesito",
        "sobre", "para", "con", "una", "unos", "unas", "los", "las", "del",
        "por", "que", "como", "donde", "desde", "pueden", "puedo", "tienen",
        "tengo", "mas", "muy", "esta", "este", "estos", "estas", "cual",
        "cuales", "favor", "gracias", "consulta", "agendar"
    };

    private static readonly HashSet<string> TeamStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "equipo", "abogado", "abogados", "abogada", "abogadas",
        "socio", "socios", "quien", "quienes", "trabaja", "trabajan",
        "trabajo", "estudio", "juridico", "juridica", "caso", "casos",
        "son", "es", "cual", "cuales", "su", "sus"
    };

    private readonly IConfiguracionEstudioService _configService;
    private readonly IMiembroEstudioService _miembroService;
    private readonly IServicioOfrecidoService _servicioService;
    private readonly ILogger<PublicChatService> _logger;

    public PublicChatService(
        IConfiguracionEstudioService configService,
        IMiembroEstudioService miembroService,
        IServicioOfrecidoService servicioService,
        ILogger<PublicChatService> logger)
    {
        _configService = configService;
        _miembroService = miembroService;
        _servicioService = servicioService;
        _logger = logger;
    }

    public async Task<PublicChatResponse> GetReplyAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(userMessage);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return DefaultReply();
        }

        if (ContainsSensitiveIntent(normalized))
        {
            return new PublicChatResponse
            {
                Message = "Por seguridad solo puedo compartir informacion publica del estudio. Si necesitas ayuda real, puedo orientarte sobre servicios, contacto o registrar una consulta.",
                SpeechText = "Por seguridad solo puedo compartir informacion publica del estudio.",
                SuggestionContext = "inicio"
            };
        }

        if (MatchesAny(normalized, "hola", "buenos", "buenas", "saludos"))
        {
            return new PublicChatResponse
            {
                Message = "Hola, con gusto te ayudo. Puedo contarte sobre nuestros servicios, datos de contacto o registrar tu consulta.",
                SuggestionContext = "inicio"
            };
        }

        var asksForContact = MatchesAny(
            normalized,
            "telefono", "celular", "numero", "llamar", "whatsapp",
            "correo", "email", "mail",
            "direccion", "ubicacion", "oficina", "contacto", "contactar",
            "donde", "ubicados", "queda", "quedan");

        var asksForServices = MatchesAny(
            normalized,
            "servicio", "servicios", "area", "areas",
            "especialidad", "especialidades", "ofrecen", "hacen");

        if (asksForServices)
        {
            return await BuildServicesReplyAsync(cancellationToken);
        }

        if (asksForContact)
        {
            return await BuildPublicContactReplyAsync(normalized, cancellationToken);
        }

        var specificServiceReply = await TryBuildMatchedServiceReplyAsync(normalized, cancellationToken);
        if (specificServiceReply is not null)
        {
            return specificServiceReply;
        }

        var teamReply = await TryBuildTeamReplyAsync(normalized, cancellationToken);
        if (teamReply is not null)
        {
            return teamReply;
        }

        if (MatchesAny(normalized, "agendar", "cita", "asesoria", "hablar con abogado", "quiero hablar", "consulta legal", "consulta", "abogado"))
        {
            return new PublicChatResponse
            {
                Message = "Perfecto, te ayudo a registrar tu consulta. Empecemos con tu nombre completo.",
                StartLeadCapture = true,
                SuggestionContext = "lead"
            };
        }

        if (MatchesAny(normalized, "horario", "horarios", "hora", "atencion", "abierto"))
        {
            return new PublicChatResponse
            {
                Message = "Nuestro horario de atencion es de lunes a viernes, de 08:30 a 17:30. Los sabados atendemos hasta las 12:00.",
                SuggestionContext = "horario"
            };
        }

        if (MatchesAny(normalized, "precio", "precios", "costo", "cuanto", "honorario", "honorarios", "cobran"))
        {
            return new PublicChatResponse
            {
                Message = "Los honorarios dependen del tipo de asunto y su complejidad. Si quieres, puedo ayudarte a registrar una consulta inicial para que el estudio te oriente mejor.",
                SuggestionContext = "precios"
            };
        }

        if (MatchesAny(normalized, "gracias", "hasta", "adios", "chao", "bye"))
        {
            return new PublicChatResponse
            {
                Message = "Fue un gusto ayudarte. Cuando quieras, aqui estare para orientarte nuevamente.",
                SuggestionContext = "inicio"
            };
        }

        return DefaultReply();
    }

    private async Task<PublicChatResponse> BuildPublicContactReplyAsync(
        string normalizedMessage,
        CancellationToken cancellationToken)
    {
        var config = await TryGetPublicConfigAsync(cancellationToken);
        if (config is null)
        {
            return new PublicChatResponse
            {
                Message = "En este momento no pude recuperar los datos publicos de contacto. Puedes usar la pagina de Contacto y con gusto registrar una consulta desde aqui.",
                SuggestionContext = "inicio"
            };
        }

        var wantsPhone = MatchesAny(normalizedMessage, "telefono", "celular", "numero", "llamar", "whatsapp");
        var wantsEmail = MatchesAny(normalizedMessage, "correo", "email", "mail");
        var wantsAddress = MatchesAny(normalizedMessage, "direccion", "ubicacion", "oficina", "donde", "ubicados", "queda", "quedan");
        var wantsAll = !wantsPhone && !wantsEmail && !wantsAddress;

        var htmlLines = new List<string>();
        var speechLines = new List<string>();

        if ((wantsAll || wantsPhone) && !string.IsNullOrWhiteSpace(config.Telefono))
        {
            htmlLines.Add($"<strong>Telefono:</strong> {Html(config.Telefono)}");
            speechLines.Add($"Telefono {config.Telefono}");
        }

        if ((wantsAll || wantsEmail) && !string.IsNullOrWhiteSpace(config.Email))
        {
            htmlLines.Add($"<strong>Correo:</strong> {Html(config.Email)}");
            speechLines.Add($"Correo {config.Email}");
        }

        if ((wantsAll || wantsAddress) && !string.IsNullOrWhiteSpace(config.Direccion))
        {
            htmlLines.Add($"<strong>Direccion:</strong> {Html(config.Direccion)}");
            speechLines.Add($"Direccion {config.Direccion}");
        }

        if (htmlLines.Count == 0)
        {
            return new PublicChatResponse
            {
                Message = "Aun no hay datos publicos de contacto configurados en el sistema. Si quieres, puedo ayudarte a registrar tu consulta ahora mismo.",
                SuggestionContext = "inicio"
            };
        }

        return new PublicChatResponse
        {
            Message =
                $"<strong>Estos son los datos publicos de contacto actuales de {Html(config.NombreEstudio)}:</strong><br>" +
                string.Join("<br>", htmlLines) +
                "<br><br>Si lo prefieres, tambien puedo ayudarte a registrar una consulta desde este chat.",
            SpeechText = $"Estos son los datos publicos de contacto actuales de {config.NombreEstudio}. " + string.Join(". ", speechLines),
            IsHtml = true,
            SuggestionContext = wantsAddress ? "ubicacion" : "contacto"
        };
    }

    private async Task<PublicChatResponse> BuildServicesReplyAsync(CancellationToken cancellationToken)
    {
        var config = await TryGetPublicConfigAsync(cancellationToken);
        var services = await TryGetPublicServicesAsync(cancellationToken);
        if (services.Count == 0)
        {
            return new PublicChatResponse
            {
                Message = string.IsNullOrWhiteSpace(config?.Direccion)
                    ? "En este momento no pude recuperar la lista de servicios publicada. Si quieres, puedo ayudarte a registrar una consulta para que el estudio te contacte."
                    : $"En este momento no pude recuperar el detalle de los servicios publicados, pero estamos ubicados en {Html(config.Direccion)}. Si quieres, puedo ayudarte a registrar una consulta para que el estudio te contacte.",
                SuggestionContext = "inicio"
            };
        }

        var publicServices = services
            .Where(service => !string.IsNullOrWhiteSpace(service.Titulo))
            .ToList();

        if (publicServices.Count == 0)
        {
            return new PublicChatResponse
            {
                Message = "Por ahora no hay servicios publicados con informacion visible. Si quieres, puedo ayudarte a registrar una consulta para orientarte mejor.",
                SuggestionContext = "inicio"
            };
        }

        var topServices = publicServices.Take(6).ToList();
        var serviceTitles = publicServices
            .Select(service => service.Titulo.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var htmlItems = topServices.Select(service =>
            $"- <strong>{Html(service.Titulo)}</strong>: {Html(Truncate(GetPublicServiceSummary(service), 140))}");

        var locationLine = string.IsNullOrWhiteSpace(config?.Direccion)
            ? string.Empty
            : $"<br><br><strong>Estamos ubicados en:</strong> {Html(config.Direccion)}.";

        return new PublicChatResponse
        {
            Message =
                $"<strong>Actualmente ofrecemos servicios en:</strong> {Html(string.Join(", ", serviceTitles))}.<br><br>" +
                string.Join("<br>", htmlItems) +
                locationLine +
                "<br><br>Puedo darte mas detalle sobre cualquiera de estas areas o ayudarte a registrar una consulta.",
            SpeechText =
                "Actualmente ofrecemos servicios en: " +
                string.Join(", ", serviceTitles) +
                ". " +
                string.Join(". ", topServices.Select(service => $"{service.Titulo}: {Truncate(GetPublicServiceSummary(service), 120)}")) +
                (string.IsNullOrWhiteSpace(config?.Direccion)
                    ? string.Empty
                    : $" Estamos ubicados en {config.Direccion}."),
            IsHtml = true,
            SuggestionContext = "servicios"
        };
    }

    private async Task<PublicChatResponse?> TryBuildTeamReplyAsync(
        string normalizedMessage,
        CancellationToken cancellationToken)
    {
        var mentionsTeamIntent = MatchesAny(
            normalizedMessage,
            "equipo", "abogado", "abogados", "abogada", "abogadas",
            "lamilla", "socios", "quien es", "quienes son")
            || Regex.IsMatch(normalizedMessage, @"\bquien(?:es)?\s+trabaja(?:n)?\b");

        if (!mentionsTeamIntent)
        {
            return null;
        }

        var members = await TryGetPublicMembersAsync(cancellationToken);
        if (members.Count == 0)
        {
            return new PublicChatResponse
            {
                Message = "En este momento no pude recuperar la informacion publica del equipo legal. Si quieres, puedo ayudarte a registrar una consulta para que el estudio te contacte.",
                SuggestionContext = "inicio"
            };
        }

        var matchedMember = FindBestMatchedMember(normalizedMessage, members);
        if (matchedMember is not null)
        {
            var bio = GetPublicMemberSummary(matchedMember);
            var reply = $"{matchedMember.Cargo} {matchedMember.Nombres} es parte fundamental de nuestro estudio. {bio}";

            return new PublicChatResponse
            {
                Message = reply,
                SpeechText = reply,
                SuggestionContext = "equipo"
            };
        }

        var mentionsGeneralTeamIntent = MatchesAny(
            normalizedMessage,
            "equipo", "abogados", "abogadas", "socios", "quienes son")
            || Regex.IsMatch(normalizedMessage, @"\bquien(?:es)?\s+trabaja(?:n)?\b");

        if (!mentionsGeneralTeamIntent)
        {
            return null;
        }

        var featuredNames = members
            .Where(member => !string.IsNullOrWhiteSpace(member.Nombres))
            .Take(3)
            .Select(member => member.Nombres.Trim())
            .ToList();

        var leadership = JoinWithAnd(featuredNames);
        var replyMessage = featuredNames.Count == 0
            ? "Contamos con un equipo de juristas de elite preparados para defender su caso con criterio estrategico y atencion personalizada."
            : $"Contamos con un equipo de juristas de elite preparados para defender su caso, liderados por {leadership}. Cada profesional aporta experiencia, criterio estrategico y atencion personalizada en las distintas areas del derecho.";

        return new PublicChatResponse
        {
            Message = replyMessage,
            SpeechText = replyMessage,
            SuggestionContext = "equipo"
        };
    }

    private async Task<PublicChatResponse?> TryBuildMatchedServiceReplyAsync(
        string normalizedMessage,
        CancellationToken cancellationToken)
    {
        var services = await TryGetPublicServicesAsync(cancellationToken);
        if (services.Count == 0)
        {
            return null;
        }

        var tokens = ExtractTokens(normalizedMessage);
        if (tokens.Count == 0)
        {
            return null;
        }

        var matches = services
            .Select(service => new
            {
                Service = service,
                Score = ScoreService(service, tokens)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Service.Orden)
            .Take(3)
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count == 1)
        {
            var match = matches[0].Service;
            var summary = GetPublicServiceSummary(match);

            return new PublicChatResponse
            {
                Message =
                    $"<strong>{Html(match.Titulo)}</strong><br>{Html(Truncate(summary, 260))}<br><br>" +
                    "Si quieres, puedo ayudarte a registrar una consulta sobre este tema.",
                SpeechText = $"{match.Titulo}. {Truncate(summary, 180)}",
                IsHtml = true,
                SuggestionContext = "servicios"
            };
        }

        var html = new StringBuilder("<strong>Encontre estas areas relacionadas con tu consulta:</strong><br>");
        foreach (var match in matches)
        {
            html.Append($"- <strong>{Html(match.Service.Titulo)}</strong>: {Html(Truncate(GetPublicServiceSummary(match.Service), 100))}<br>");
        }

        html.Append("<br>Si quieres, tambien puedo ayudarte a registrar una consulta para que el estudio te atienda.");

        return new PublicChatResponse
        {
            Message = html.ToString(),
            SpeechText = "Encontre areas relacionadas con tu consulta: " +
                         string.Join(". ", matches.Select(x => x.Service.Titulo)),
            IsHtml = true,
            SuggestionContext = "servicios"
        };
    }

    private async Task<PublicStudioInfo?> TryGetPublicConfigAsync(CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configService.GetAsync(cancellationToken);
            return new PublicStudioInfo(
                TrimToNull(config.NombreEstudio) ?? "Lamilla Escudero & Asociados",
                TrimToNull(config.TelefonoContacto),
                TrimToNull(config.EmailContacto),
                TrimToNull(config.Direccion));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo recuperar la configuracion publica del estudio para el chat.");
            return null;
        }
    }

    private async Task<List<MiembroEstudioResponse>> TryGetPublicMembersAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _miembroService.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo recuperar el equipo publico del estudio para el chat.");
            return new List<MiembroEstudioResponse>();
        }
    }

    private async Task<List<ServicioOfrecidoResponse>> TryGetPublicServicesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _servicioService.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron recuperar los servicios publicos del estudio para el chat.");
            return new List<ServicioOfrecidoResponse>();
        }
    }

    private static bool ContainsSensitiveIntent(string normalizedInput)
        => SensitiveKeywords.Any(keyword => normalizedInput.Contains(keyword, StringComparison.Ordinal));

    private static bool MatchesAny(string input, params string[] keywords)
        => keywords.Any(keyword => input.Contains(keyword, StringComparison.Ordinal));

    private static string Normalize(string value)
    {
        var lower = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(lower.Length);

        foreach (var character in lower)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static List<string> ExtractTokens(string normalizedInput)
        => Regex.Matches(normalizedInput, @"[\p{L}\p{N}]+")
            .Select(match => match.Value)
            .Where(token => token.Length >= 4 && !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<string> ExtractMemberTokens(string normalizedInput)
        => Regex.Matches(normalizedInput, @"[\p{L}\p{N}]+")
            .Select(match => match.Value)
            .Where(token => token.Length >= 3 && !StopWords.Contains(token) && !TeamStopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static int ScoreService(ServicioOfrecidoResponse service, IEnumerable<string> tokens)
    {
        var searchableText = Normalize($"{service.Titulo} {service.DescripcionCorta} {service.Detalles}");
        var score = 0;

        foreach (var token in tokens)
        {
            if (searchableText.Contains(token, StringComparison.Ordinal))
            {
                score++;
            }
        }

        return score;
    }

    private static MiembroEstudioResponse? FindBestMatchedMember(
        string normalizedMessage,
        IEnumerable<MiembroEstudioResponse> members)
    {
        var tokens = ExtractMemberTokens(normalizedMessage);
        if (tokens.Count == 0)
        {
            return null;
        }

        return members
            .Select(member => new
            {
                Member = member,
                Score = ScoreMember(member, tokens, normalizedMessage)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Member.Orden)
            .Select(x => x.Member)
            .FirstOrDefault();
    }

    private static int ScoreMember(
        MiembroEstudioResponse member,
        IEnumerable<string> tokens,
        string normalizedMessage)
    {
        var searchableName = Normalize(member.Nombres);
        if (string.IsNullOrWhiteSpace(searchableName))
        {
            return 0;
        }

        var score = normalizedMessage.Contains(searchableName, StringComparison.Ordinal) ? 5 : 0;

        foreach (var token in tokens)
        {
            if (searchableName.Contains(token, StringComparison.Ordinal))
            {
                score++;
            }
        }

        return score;
    }

    private static string Html(string? value)
        => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string GetPublicMemberSummary(MiembroEstudioResponse member)
        => FirstNonEmpty(member.BiografiaBreve)
            ?? "Integra nuestro equipo legal con experiencia, criterio estrategico y compromiso directo con la defensa de nuestros clientes.";

    private static string GetPublicServiceSummary(ServicioOfrecidoResponse service)
        => FirstNonEmpty(service.DescripcionCorta, service.Detalles, service.Titulo) ?? service.Titulo;

    private static string Truncate(string? value, int maxLength)
    {
        var clean = (value ?? string.Empty).Trim();
        if (clean.Length <= maxLength)
        {
            return clean;
        }

        return clean[..maxLength].TrimEnd() + "...";
    }

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static string JoinWithAnd(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        if (values.Count == 1)
        {
            return values[0];
        }

        if (values.Count == 2)
        {
            return $"{values[0]} y {values[1]}";
        }

        return $"{string.Join(", ", values.Take(values.Count - 1))} y {values[^1]}";
    }

    private static PublicChatResponse DefaultReply()
        => new()
        {
            Message = "Puedo ayudarte con informacion sobre servicios, contacto del estudio o registrar una consulta. Si quieres, dime en que tema necesitas orientacion.",
            SuggestionContext = "inicio"
        };

    private sealed record PublicStudioInfo(
        string NombreEstudio,
        string? Telefono,
        string? Email,
        string? Direccion);
}
