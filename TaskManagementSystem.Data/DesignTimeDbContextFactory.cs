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
                    "The EF Core tooling cannot read the application's appsettings.json, so it needs the " +
                    "connection string here, for example " +
                    "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=... " +
                    "Note that setx only affects new processes, so reopen the terminal or restart Visual Studio. " +
                    "If you only need the schema, running db/schema.sql requires none of this - see the README.");
            }

            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new TaskManagerDbContext(options);
        }
    }
}
