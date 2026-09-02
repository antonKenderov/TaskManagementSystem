using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IDbContextFactory<TaskManagerDbContext> _dbContextFactory;

        public TaskService(IDbContextFactory<TaskManagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<TaskTableItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await db.TaskItems
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TaskTableItemDto(
                    t.Id,
                    t.CreatedAt,
                    t.RequiredByDate,
                    t.Description,
                    t.Type,
                    t.Status,
                    t.AssignedTo != null ? t.AssignedTo.Name : "Unassigned",
                    t.Comments.Min(c => c.ReminderDate)))
                .ToListAsync(cancellationToken);
        }
    }
}
