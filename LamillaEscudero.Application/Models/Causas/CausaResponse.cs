namespace LamillaEscudero.Application.Models.Causas;

public class CausaResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;

    // Nullable: causas penales en fase de investigación no tienen proceso aún
    public string? NumeroProceso { get; set; }

    // Expediente de Fiscalía
    public string? ExpedienteFiscal { get; set; }

    public string? UnidadJudicial { get; set; }
    public string? Materia { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public string? Resumen { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // ══════════════════════════════════════════════════════════════
    //  IDENTIFICADOR VISUAL "CAMALEÓN"
    //  Decide qué mostrar según el estado de la causa:
    //
    //  Civil/Familia → "17250-2024-00123"
    //  Penal investigación (solo Fiscalía) → "Fiscalía: 065-2024"
    //  Penal instrucción (tiene juez + fiscal) → "Proc: 17250-2024 | Exp: 065-2024"
    //  Penal sin nada → "Sin identificador"
    // ══════════════════════════════════════════════════════════════
    public string IdentificadorVisual
    {
        get
        {
            bool esPenal = string.Equals(Materia, "Penal", StringComparison.OrdinalIgnoreCase)
                        || (Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true);

            bool tieneProceso = !string.IsNullOrWhiteSpace(NumeroProceso);
            bool tieneExpFiscal = !string.IsNullOrWhiteSpace(ExpedienteFiscal);

            if (!esPenal)
            {
                // Civil / Familia / Laboral — muestra número de proceso directo
                return tieneProceso ? NumeroProceso! : "Sin número de proceso";
            }

            // Causa Penal
            if (tieneProceso && tieneExpFiscal)
                return $"Proc: {NumeroProceso} | Exp: {ExpedienteFiscal}";

            if (tieneProceso)
                return $"Proceso: {NumeroProceso}";

            if (tieneExpFiscal)
                return $"Fiscalía: {ExpedienteFiscal}";

            return "Penal — sin identificador";
        }
    }

    // Etiqueta corta para badges en tablas
    public string EtiquetaAmbito
    {
        get
        {
            bool esPenal = Materia?.Contains("penal", StringComparison.OrdinalIgnoreCase) == true;
            if (!esPenal) return "Civil";

            bool tieneProceso = !string.IsNullOrWhiteSpace(NumeroProceso);
            return tieneProceso ? "Penal — Instrucción" : "Penal — Investigación";
        }
    }
}
