using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Data;
using TaskManagementSystem.Domain;

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
            TaskFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = db.TaskItems.AsNoTracking();

            if (filter?.Status is not null)
                query = query.Where(t => t.Status == filter.Status);

            if (filter?.Type is not null)
                query = query.Where(t => t.Type == filter.Type);

            if (filter?.AssignedToUserId is not null)
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

            return await query
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

        public async Task<int> CreateTaskAsync(NewTaskDto task, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(task.Description)) throw new ArgumentException("Description cannot be empty.", nameof(task.Description));
            if (task.Description.Length > 2000) throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(task.Description));
            if (task.RequiredByDate < DateOnly.FromDateTime(DateTime.Now)) throw new ArgumentException("RequiredByDate cannot be in the past.", nameof(task.RequiredByDate));

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var newTask = new TaskItem
            {
                RequiredByDate = task.RequiredByDate,
                Description = task.Description,
                Status = task.Status,
                Type = task.Type,
                AssignedToUserId = task.AssignedToUserId
            };

            db.TaskItems.Add(newTask);
            await db.SaveChangesAsync(cancellationToken);

            return newTask.Id;
        }

        public async Task<TaskDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await db.TaskItems
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TaskDetailDto
                {
                    Id = t.Id,
                    CreatedAt = t.CreatedAt,
                    RequiredBy = t.RequiredByDate,
                    Description = t.Description,
                    Status = t.Status,
                    Type = t.Type,
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedTo = t.AssignedTo != null
                        ? new UserDto(t.AssignedTo.Id, t.AssignedTo.Name)
                        : null,
                    ModifiedAt = t.ModifiedAt,
                    ModifiedBy = t.ModifiedBy,
                    Comments = t.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new CommentDto
                        {
                            Id = c.Id,
                            Text = c.Text,
                            CreatedAt = c.CreatedAt,
                            Type = c.Type,
                            ReminderDate = c.ReminderDate,
                            ModifiedAt = c.ModifiedAt,
                            ModifiedBy = c.ModifiedBy
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateTaskAsync(UpdateTaskDto task, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(task.Description)) throw new ArgumentException("Description cannot be empty.", nameof(task.Description));
            if (task.Description.Length > 2000) throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(task.Description));

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await db.TaskItems.FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Task {task.Id} no longer exists.");

            existing.Description = task.Description;
            existing.RequiredByDate = task.RequiredByDate;
            existing.Status = task.Status;
            existing.Type = task.Type;
            existing.AssignedToUserId = task.AssignedToUserId;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
