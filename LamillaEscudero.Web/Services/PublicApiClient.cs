using System.Net.Http.Json;
using LamillaEscudero.Web.Models;

namespace LamillaEscudero.Web.Services;

/// <summary>
/// Cliente HTTP para endpoints públicos de la API.
/// Actualizado en Fase 7: ConfiguracionPublicaDto incluye campos de mapa.
/// </summary>
public class PublicApiClient
{
    private readonly HttpClient _http;

    public PublicApiClient(HttpClient http) => _http = http;

    public async Task<ConfiguracionPublicaDto?> GetConfiguracionAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<ConfiguracionPublicaDto>("api/public/configuracion", ct);

    public async Task<ConsultaPublicaResponse?> EnviarConsultaAsync(
        ConsultaPublicaRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/public/consultas", request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ConsultaPublicaResponse>(ct);
    }
}
