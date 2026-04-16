using LamillaEscudero.Application.Models.Dashboard;

namespace LamillaEscudero.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}