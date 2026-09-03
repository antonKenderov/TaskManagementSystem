
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
            if (string.IsNullOrWhiteSpace(comment.Text)) throw new ArgumentException("Comment cannot be empty.", nameof(comment.Text));
            if (comment.Text.Length > 3000) throw new ArgumentException("Comment cannot exceed 3000 characters.", nameof(comment.Text));

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
    }
}
