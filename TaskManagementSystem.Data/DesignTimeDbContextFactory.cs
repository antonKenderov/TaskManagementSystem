using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManagementSystem.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskManagerDbContext>
    {
        private const string ConnectionStringVariable = "TASKMANAGER_CONNECTION";

        public TaskManagerDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"The {ConnectionStringVariable} environment variable is not set. " +
                    "Set it to the database connection string before running EF Core commands, e.g. " +
                    "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=...");
            }

            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new TaskManagerDbContext(options);
        }
    }
}
