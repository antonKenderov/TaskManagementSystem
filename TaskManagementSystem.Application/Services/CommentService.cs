
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Data;
using TaskManagementSystem.Domain;

namespace TaskManagementSystem.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IDbContextFactory<TaskManagerDbContext> _dbContextFactory;

        public CommentService(IDbContextFactory<TaskManagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> CreateCommentAsync(NewCommentDto comment, CancellationToken cancellationToken = default)
        {
            ValidateText(comment.Text);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            if (!await db.TaskItems.AnyAsync(t => t.Id == comment.TaskItemId, cancellationToken))
                throw new InvalidOperationException($"Task {comment.TaskItemId} no longer exists.");

            var newComment = new Comment
            {
                TaskItemId = comment.TaskItemId,
                Text = comment.Text,
                Type = comment.Type,
                ReminderDate = comment.ReminderDate
            };

            db.Comments.Add(newComment);
            await db.SaveChangesAsync(cancellationToken);

            return newComment.Id;
        }

        public async Task DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var deleted = await db.Comments
                                .Where(c => c.Id == commentId)
                                .ExecuteDeleteAsync(cancellationToken);

            if (deleted == 0)
                throw new InvalidOperationException($"Comment {commentId} no longer exists.");
        }

        public async Task UpdateCommentAsync(UpdateCommentDto comment, CancellationToken cancellationToken = default)
        {
            ValidateText(comment.Text);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Comment {comment.Id} no longer exists.");

            existing.Text = comment.Text;
            existing.Type = comment.Type;
            existing.ReminderDate = comment.ReminderDate;

            await db.SaveChangesAsync(cancellationToken);
        }

        private static void ValidateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Comment cannot be empty.", nameof(text));
            if (text.Length > 3000) throw new ArgumentException("Comment cannot exceed 3000 characters.", nameof(text));
        }

        public async Task<IReadOnlyList<CommentSearchResultDto>> SearchCommentsAsync(CommentSearchFilter filter, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = db.Comments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Text))
                query = query.Where(c => EF.Functions.ILike(c.Text, $"%{filter.Text}%"));

            if (filter.Type is not null)
                query = query.Where(c => c.Type == filter.Type);

            if (filter.ReminderFrom is not null)
                query = query.Where(c => c.ReminderDate >= filter.ReminderFrom);

            if (filter.ReminderTo is not null)
                query = query.Where(c => c.ReminderDate <= filter.ReminderTo);

            if (filter.AddedFrom is not null)
                query = query.Where(c => c.CreatedAt >= filter.AddedFrom);

            if (filter.AddedTo is not null)
                query = query.Where(c => c.CreatedAt <= filter.AddedTo);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentSearchResultDto
                {
                    Id = c.Id,
                    TaskItemId = c.TaskItemId,
                    TaskDescription = c.TaskItem.Description,
                    Text = c.Text,
                    Type = c.Type,
                    CreatedAt = c.CreatedAt,
                    ReminderDate = c.ReminderDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
