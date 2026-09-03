using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IReadOnlyList<TaskTableItemDto>> GetAllAsync(TaskFilter? filter = null, CancellationToken cancellationToken = default);
        Task<int> CreateTaskAsync(NewTaskDto task, CancellationToken cancellationToken = default);
        Task<TaskDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateTaskAsync(UpdateTaskDto task, CancellationToken cancellationToken = default);
    }
}
