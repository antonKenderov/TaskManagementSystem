using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.Services;
using TaskManagementSystem.Data;
using TaskManagementSystem.Domain;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagement.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class TaskServiceTests
    {
        private const int SeededUserId = 1;

        private readonly PostgreSqlFixture _fixture;

        public TaskServiceTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReportEarliestReminderAsNextActionDate()
        {
            // Arrange
            var earliest = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            var latest = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9));

            var withReminders = NewTask("Task carrying reminders");
            withReminders.Comments.Add(new Comment { Text = "later", Type = CommentType.InternalNote, ReminderDate = latest });
            withReminders.Comments.Add(new Comment { Text = "sooner", Type = CommentType.Blocker, ReminderDate = earliest });
            withReminders.Comments.Add(new Comment { Text = "no reminder", Type = CommentType.ClientUpdate, ReminderDate = null });

            var withoutReminders = NewTask("Task with comments but no reminders");
            withoutReminders.Comments.Add(new Comment { Text = "just a note", Type = CommentType.InternalNote, ReminderDate = null });

            var withoutComments = NewTask("Task with no comments at all");

            await using (var writeContext = _fixture.CreateContext())
            {
                writeContext.TaskItems.AddRange(withReminders, withoutReminders, withoutComments);
                await writeContext.SaveChangesAsync();
            }

            var service = new TaskService(new TestDbContextFactory(_fixture));

            // Act
            var result = await service.GetAllAsync();

            // Assert
            var a = result.Single(t => t.Id == withReminders.Id);
            var b = result.Single(t => t.Id == withoutReminders.Id);
            var c = result.Single(t => t.Id == withoutComments.Id);

            Assert.Equal(earliest, a.NextActionDate);
            Assert.Null(b.NextActionDate);
            Assert.Null(c.NextActionDate);

            Assert.Equal("Anton", a.AssignedToName);
            Assert.Equal(Status.Open, a.Status);
            Assert.Equal(TaskType.Maintenance, a.Type);
        }

        private static TaskItem NewTask(string description) => new()
        {
            Description = description,
            Status = Status.Open,
            Type = TaskType.Maintenance,
            RequiredByDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)),
            AssignedToUserId = SeededUserId
        };

        private sealed class TestDbContextFactory : IDbContextFactory<TaskManagerDbContext>
        {
            private readonly PostgreSqlFixture _fixture;

            public TestDbContextFactory(PostgreSqlFixture fixture) => _fixture = fixture;

            public TaskManagerDbContext CreateDbContext() => _fixture.CreateContext();
        }
    }
}
