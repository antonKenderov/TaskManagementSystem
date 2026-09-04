using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.ViewModels
{
    public partial class TasksViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;

        private bool _suppressFilterReload;

        public TasksViewModel(
            ITaskService taskService,
            IUserService userService,
            TaskDetailViewModel taskDetail,
            NewTaskViewModel newTask)
        {
            _taskService = taskService;
            _userService = userService;
            TaskDetail = taskDetail;
            NewTask = newTask;

            NewTask.TaskCreated = LoadTasksAsync;
        }

        public TaskDetailViewModel TaskDetail { get; }

        public NewTaskViewModel NewTask { get; }

        public ObservableCollection<TaskTableItemDto> Tasks { get; } = new();

        public ObservableCollection<FilterOption> UserFilterOptions { get; } = new();

        public IEnumerable<FilterOption> StatusFilterOptions =>
            new[] { new FilterOption("All", null) }
                .Concat(Enum.GetValues<Status>().Select(v => new FilterOption(v.ToString(), v)));

        public IEnumerable<FilterOption> TypeFilterOptions =>
            new[] { new FilterOption("All", null) }
                .Concat(Enum.GetValues<TaskType>().Select(v => new FilterOption(v.ToString(), v)));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private Status? _filterStatus;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private TaskType? _filterType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private int? _filterUserId;

        [ObservableProperty]
        private TaskTableItemDto? _selectedTask;

        [ObservableProperty]
        private bool _isDetailsVisible;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadTasksCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasActiveFilters =>
            FilterStatus is not null || FilterType is not null || FilterUserId is not null;

        private bool CanLoadTasks => !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLoadTasks))]
        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var data = await _taskService.GetAllAsync(new TaskFilter
                {
                    Status = FilterStatus,
                    Type = FilterType,
                    AssignedToUserId = FilterUserId
                });

                Tasks.Clear();
                foreach (var item in data)
                {
                    Tasks.Add(item);
                }
            }
            catch (Exception ex)
            {
                Tasks.Clear();
                ErrorMessage = $"Could not load the tasks: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadFilterUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();

                UserFilterOptions.Clear();
                UserFilterOptions.Add(new FilterOption("Anyone", null));
                foreach (var user in users)
                {
                    UserFilterOptions.Add(new FilterOption(user.Name, user.Id));
                }
            }
            catch (Exception ex)
            {
                UserFilterOptions.Clear();
                ErrorMessage = $"Could not load the users: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            _suppressFilterReload = true;
            FilterStatus = null;
            FilterType = null;
            FilterUserId = null;
            _suppressFilterReload = false;

            ReloadForFilter();
        }

        [RelayCommand]
        private async Task OpenTaskDetailAsync()
        {
            if (SelectedTask is null)
            {
                return;
            }

            await OpenTaskAsync(SelectedTask.Id);
        }

        public async Task OpenTaskAsync(int taskId)
        {
            await TaskDetail.LoadAsync(taskId);
            IsDetailsVisible = true;
        }

        [RelayCommand]
        private async Task CloseDetailAsync()
        {
            IsDetailsVisible = false;
            await LoadTasksAsync();
        }

        partial void OnFilterStatusChanged(Status? value) => ReloadForFilter();

        partial void OnFilterTypeChanged(TaskType? value) => ReloadForFilter();

        partial void OnFilterUserIdChanged(int? value) => ReloadForFilter();

        private void ReloadForFilter()
        {
            if (_suppressFilterReload)
            {
                return;
            }

            if (LoadTasksCommand.CanExecute(null))
            {
                LoadTasksCommand.Execute(null);
            }
        }
    }
}
