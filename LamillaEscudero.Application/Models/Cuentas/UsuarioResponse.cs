

namespace LamillaEscudero.Application.Models.Cuentas
{
    public class UsuarioResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Cedula { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public Guid? ClienteVinculadoId { get; set; }
        public string? ClienteVinculadoNombre { get; set; }
    }
}
