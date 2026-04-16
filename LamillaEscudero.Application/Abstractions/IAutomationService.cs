namespace LamillaEscudero.Application.Abstractions;

public interface IAutomationService
{
    Task EjecutarAsync(CancellationToken cancellationToken = default);
}