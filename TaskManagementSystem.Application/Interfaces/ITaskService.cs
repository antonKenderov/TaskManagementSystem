using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IReadOnlyList<TaskTableItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
