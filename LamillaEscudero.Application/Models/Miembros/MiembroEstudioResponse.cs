
// ══════════════════════════════════════════════════════════════════
//  MiembroEstudioResponse.cs  —  Fase 9
// ══════════════════════════════════════════════════════════════════
namespace LamillaEscudero.Application.Models.Miembros
{
    public class MiembroEstudioResponse
    {
        public Guid Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string? BiografiaBreve { get; set; }
        public string? BiografiaCompleta { get; set; }

        // foto
        public string? FotoUrl { get; set; }
        public string? PhotoData { get; set; }   // ← FASE 9 (Base64)

        // perfil extendido
        public string? FraseDestacada { get; set; }   // ← FASE 9
        public string? BiografiaLarga { get; set; }   // ← FASE 9
        public string? EducacionJson { get; set; }   // ← FASE 9
        public string? TimelineExperienciaJson { get; set; }   // ← FASE 9

        public string? LinkedIn { get; set; }
        public string? Email { get; set; }
        public int Orden { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // ── helpers de presentación ───────────────────────────
        /// <summary>Fuente de imagen resuelta: Base64 tiene prioridad sobre URL.</summary>
        public string? ImagenSrc =>
            !string.IsNullOrWhiteSpace(PhotoData) ? PhotoData :
            !string.IsNullOrWhiteSpace(FotoUrl) ? FotoUrl : null;
    }
}
