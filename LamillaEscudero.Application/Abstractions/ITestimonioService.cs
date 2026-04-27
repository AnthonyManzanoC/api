using LamillaEscudero.Application.Models.Testimonios;

namespace LamillaEscudero.Application.Abstractions;

public interface ITestimonioService
{
    Task<List<TestimonioResponse>> GetAprobadosAsync(CancellationToken ct = default);
    Task<List<TestimonioResponse>> GetPendientesAsync(CancellationToken ct = default);
    Task<List<TestimonioResponse>> GetAllAsync(CancellationToken ct = default);
    Task<TestimonioResponse> CreateAsync(TestimonioCreateRequest request, CancellationToken ct = default);
    Task<bool> ModerarAsync(Guid id, TestimonioUpdateEstadoRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}