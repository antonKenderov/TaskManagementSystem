using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private const int UpcomingDays = 14;

        private readonly IDashboardService _dashboardService;

        public DashboardViewModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public ObservableCollection<StatusCountDto> StatusBreakdown { get; } = new();

        public ObservableCollection<TaskTableItemDto> UpcomingDeadlines { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalTasks))]
        [NotifyPropertyChangedFor(nameof(InProgressCount))]
        [NotifyPropertyChangedFor(nameof(CompletedCount))]
        [NotifyPropertyChangedFor(nameof(DueThisWeekCount))]
        private DashboardDto? _summary;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        public int TotalTasks => Summary?.TotalTasks ?? 0;

        public int InProgressCount => Summary?.InProgressCount ?? 0;

        public int CompletedCount => Summary?.CompletedCount ?? 0;

        public int DueThisWeekCount => Summary?.DueThisWeekCount ?? 0;

        public string UpcomingDaysLabel => $"Next {UpcomingDays} days";

        private bool CanLoad => !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLoad))]
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var summary = await _dashboardService.GetSummaryAsync();
                var upcoming = await _dashboardService.GetUpcomingDeadlinesAsync(UpcomingDays);

                Summary = summary;

                StatusBreakdown.Clear();
                foreach (var entry in summary.StatusBreakdown)
                {
                    StatusBreakdown.Add(entry);
                }

                UpcomingDeadlines.Clear();
                foreach (var task in upcoming)
                {
                    UpcomingDeadlines.Add(task);
                }
            }
            catch (Exception ex)
            {
                Summary = null;
                StatusBreakdown.Clear();
                UpcomingDeadlines.Clear();
                ErrorMessage = $"Could not load the dashboard: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
