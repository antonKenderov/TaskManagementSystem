
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Data;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private const int DueSoonDays = 7;
        private readonly IDbContextFactory<TaskManagerDbContext> _dbContextFactory;

        public DashboardService(IDbContextFactory<TaskManagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<DashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var counts = await db.TaskItems
                .AsNoTracking()
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var dueSoonCutoff = DateOnly.FromDateTime(DateTime.Now).AddDays(DueSoonDays);

            var dueThisWeek = await db.TaskItems
                .AsNoTracking()
                .CountAsync(t => t.Status != Status.Closed && t.RequiredByDate <= dueSoonCutoff, cancellationToken);

            var breakdown = Enum.GetValues<Status>()
                 .Select(status => new StatusCountDto
                 {
                     Status = status,
                     Count = counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0
                 })
                 .ToList();

            return new DashboardDto
            {
                TotalTasks = counts.Sum(c => c.Count),
                InProgressCount = breakdown.First(b => b.Status == Status.InProgress).Count,
                CompletedCount = breakdown.First(b => b.Status == Status.Closed).Count,
                DueThisWeekCount = dueThisWeek,
                StatusBreakdown = breakdown
            };
        }

        public async Task<IReadOnlyList<TaskTableItemDto>> GetUpcomingDeadlinesAsync(int days = 14, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var cutoff = DateOnly.FromDateTime(DateTime.Now).AddDays(days);

            return await db.TaskItems
                .AsNoTracking()
                .Where(t => t.Status != Status.Closed && t.RequiredByDate <= cutoff)
                .OrderBy(t => t.RequiredByDate)
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
