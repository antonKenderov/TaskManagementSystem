using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagement.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class AuditStampingTests
    {
        private const int SeededUserId = 1;

        private readonly PostgreSqlFixture _fixture;

        public AuditStampingTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddingTask_ShouldStampCreatedAt_AndLeaveModifiedEmpty()
        {
            // Arrange
            var before = DateTime.UtcNow;
            var task = NewTask("A task that is only ever created");

            // Act
            await using (var writeContext = _fixture.CreateContext())
            {
                writeContext.TaskItems.Add(task);
                await writeContext.SaveChangesAsync();
            }

            var after = DateTime.UtcNow;

            TaskItem? saved;
            await using (var readContext = _fixture.CreateContext())
            {
                saved = await readContext.TaskItems.SingleOrDefaultAsync(t => t.Id == task.Id);
            }

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(DateTimeKind.Utc, saved.CreatedAt.Kind);
            Assert.InRange(saved.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));

            Assert.Null(saved.ModifiedAt);
            Assert.Null(saved.ModifiedBy);
        }

        [Fact]
        public async Task UpdatingTask_ShouldStampModified_AndPreserveCreatedAt()
        {
            // Arrange
            var task = NewTask("Original description");

            await using (var writeContext = _fixture.CreateContext())
            {
                writeContext.TaskItems.Add(task);
                await writeContext.SaveChangesAsync();
            }

            DateTime createdAtAfterInsert;
            await using (var readContext = _fixture.CreateContext())
            {
                createdAtAfterInsert = (await readContext.TaskItems.SingleAsync(t => t.Id == task.Id)).CreatedAt;
            }

            // Act
            await using (var updateContext = _fixture.CreateContext())
            {
                var toEdit = await updateContext.TaskItems.SingleAsync(t => t.Id == task.Id);
                toEdit.Description = "Edited description";
                await updateContext.SaveChangesAsync();
            }

            TaskItem edited;
            await using (var readContext = _fixture.CreateContext())
            {
                edited = await readContext.TaskItems.SingleAsync(t => t.Id == task.Id);
            }

            // Assert
            Assert.Equal("Edited description", edited.Description);

            Assert.NotNull(edited.ModifiedAt);
            Assert.Equal(DateTimeKind.Utc, edited.ModifiedAt!.Value.Kind);
            Assert.Equal(Environment.UserName, edited.ModifiedBy);

            Assert.Equal(createdAtAfterInsert, edited.CreatedAt);
            Assert.True(edited.ModifiedAt >= edited.CreatedAt);
        }

        private static TaskItem NewTask(string description) => new()
        {
            Description = description,
            Status = Status.Open,
            Type = TaskType.Maintenance,
            RequiredByDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            AssignedToUserId = SeededUserId
        };
    }
}
