using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Application.Services;
using TaskManagementSystem.Data;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        public static IConfiguration Config { get; private set; } = null!;

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Config = BuildConfiguration();

            var services = new ServiceCollection();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Config.GetConnectionString("DatabaseConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DatabaseConnection' is missing from appsettings.json.");

            services.AddDbContextFactory<TaskManagerDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddSingleton<ITaskService, TaskService>();
            services.AddSingleton<IUserService, UserService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<TasksViewModel>();

            services.AddSingleton<MainWindow>();
        }
    }
}
