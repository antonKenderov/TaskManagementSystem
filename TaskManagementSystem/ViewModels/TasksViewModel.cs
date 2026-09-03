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

        public TasksViewModel(ITaskService taskService, IUserService userService)
        {
            _taskService = taskService;
            _userService = userService;
        }

        public ObservableCollection<TaskTableItemDto> Tasks { get; } = new();
        public ObservableCollection<UserDto> Users { get; } = new();
        public IEnumerable<Status> Statuses => Enum.GetValues<Status>();
        public IEnumerable<TaskType> Types => Enum.GetValues<TaskType>();

        [ObservableProperty]
        private UserDto? _selectedUser;

        [ObservableProperty]
        private Status _selectedStatus = Status.Open;

        [ObservableProperty]
        private TaskType _selectedType = TaskType.FeatureRequest;

        [ObservableProperty]
        private TaskTableItemDto? _selectedTask;

        public DateTime CurrentTime => DateTime.Now;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadTasksCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isPopupOpen;

        [ObservableProperty]
        private string? _errorMessage;

        private bool CanLoadTasks => !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLoadTasks))]
        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var data = await _taskService.GetAllAsync();

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
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                SelectedUser ??= Users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Users.Clear();
                SelectedUser = null;
                ErrorMessage = $"Could not load the users: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task OpenPopupAsync()
        {
            // The assignee list is only needed by the form, so it is fetched when
            // the form opens rather than kept in step with the task list.
            await LoadUsersAsync();
            IsPopupOpen = true;
        }

        [RelayCommand]
        private void ClosePopup()
        {
            IsPopupOpen = false;
        }
    }
}
