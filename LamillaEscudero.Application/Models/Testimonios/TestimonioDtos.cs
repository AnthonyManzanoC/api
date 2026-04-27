using System.ComponentModel.DataAnnotations;

namespace LamillaEscudero.Application.Models.Testimonios;

public class TestimonioCreateRequest
{
    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Comentario { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Calificacion { get; set; } = 5;
}

public class TestimonioUpdateEstadoRequest
{
    public string Estado { get; set; } = "Aprobado";
    public string? NotaAdmin { get; set; }
}

public class TestimonioResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool FlagHate { get; set; }
    public string? NotaAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
}