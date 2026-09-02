using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using Testcontainers.PostgreSql;

namespace TaskManagement.Tests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        public DbContextOptions<TaskManagerDbContext> DbOptions { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            DbOptions = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            using var context = CreateContext();
            await context.Database.MigrateAsync();
        }

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();
        public TaskManagerDbContext CreateContext() => new(DbOptions);
    }
}
