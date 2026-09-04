
using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TaskTableItemDto>> GetUpcomingDeadlinesAsync(int days = 14, CancellationToken cancellationToken = default);
    }
}
