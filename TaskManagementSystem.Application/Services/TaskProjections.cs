using System.Linq.Expressions;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Domain;

namespace TaskManagementSystem.Application.Services
{
    /// <summary>
    /// Shared read projections. These are expressions rather than delegates, so
    /// EF Core can still translate them into SQL when used inside a query.
    /// </summary>
    internal static class TaskProjections
    {
        public static readonly Expression<Func<TaskItem, TaskTableItemDto>> ToTableItem =
            task => new TaskTableItemDto(
                task.Id,
                task.CreatedAt,
                task.RequiredByDate,
                task.Description,
                task.Type,
                task.Status,
                task.AssignedTo != null ? task.AssignedTo.Name : "Unassigned",
                task.Comments.Min(comment => comment.ReminderDate));
    }
}
