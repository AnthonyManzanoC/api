

namespace LamillaEscudero.Application.Models.Cuentas
{
    public class CrearUsuarioRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string Rol { get; set; } = "Asistente";
        // Solo se usa cuando Rol == "Cliente"
        public Guid? ClienteId { get; set; }
    }
}
