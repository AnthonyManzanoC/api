using System.ComponentModel.DataAnnotations;

namespace LamillaEscudero.Web.Models;

public class ConsultaPublicaRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El asunto es obligatorio")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es obligatorio")]
    public string Mensaje { get; set; } = string.Empty;
}