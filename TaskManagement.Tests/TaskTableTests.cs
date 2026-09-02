using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagement.Tests
{
    public class TaskTableTests : IClassFixture<PostgreSqlFixture>
    {
        private const int SeededUserId = 1;

        private readonly PostgreSqlFixture _fixture;

        public TaskTableTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SaveTask_WithComments_ShouldPersistAndReadCorrectly()
        {
            // Arrange
            var requiredByDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            var reminderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

            var taskToSave = new TaskItem
            {
                Description = "Fix the bug in the login form",
                Status = Status.InProgress,
                Type = TaskType.BugReport,
                RequiredByDate = requiredByDate,
                AssignedToUserId = SeededUserId,
                Comments =
                {
                    new Comment { Text = "Comment 1 - with a reminder", Type = CommentType.Blocker, ReminderDate = reminderDate },
                    new Comment { Text = "Comment 2 - without a reminder", Type = CommentType.InternalNote, ReminderDate = null }
                }
            };

            // Act
            await using (var writeContext = _fixture.CreateContext())
            {
                writeContext.TaskItems.Add(taskToSave);
                await writeContext.SaveChangesAsync();
            }

            TaskItem? readTask;
            await using (var readContext = _fixture.CreateContext())
            {
                readTask = await readContext.TaskItems
                    .Include(t => t.Comments)
                    .SingleOrDefaultAsync(t => t.Id == taskToSave.Id);
            }

            // Assert
            Assert.NotNull(readTask);
            Assert.Equal("Fix the bug in the login form", readTask.Description);
            Assert.Equal(Status.InProgress, readTask.Status);
            Assert.Equal(TaskType.BugReport, readTask.Type);
            Assert.Equal(SeededUserId, readTask.AssignedToUserId);
            Assert.Equal(requiredByDate, readTask.RequiredByDate);

            Assert.Equal(2, readTask.Comments.Count);

            var commentWithReminder = readTask.Comments.Single(c => c.Text.Contains("with a reminder"));
            var commentWithoutReminder = readTask.Comments.Single(c => c.Text.Contains("without a reminder"));

            Assert.Equal(reminderDate, commentWithReminder.ReminderDate);
            Assert.Null(commentWithoutReminder.ReminderDate);

            Assert.Equal(CommentType.Blocker, commentWithReminder.Type);
            Assert.Equal(CommentType.InternalNote, commentWithoutReminder.Type);

            // CreatedAt is stamped by SaveChanges, so it must come back set and in UTC.
            Assert.NotEqual(default, readTask.CreatedAt);
            Assert.Equal(DateTimeKind.Utc, readTask.CreatedAt.Kind);
            Assert.NotEqual(default, commentWithReminder.CreatedAt);
        }
    }
}
