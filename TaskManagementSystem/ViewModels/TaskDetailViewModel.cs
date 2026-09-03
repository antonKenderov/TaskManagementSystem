using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.ViewModels
{
    public partial class TaskDetailViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;

        public TaskDetailViewModel(ITaskService taskService, IUserService userService)
        {
            _taskService = taskService;
            _userService = userService;
        }

        public ObservableCollection<CommentDto> Comments { get; } = new();
        public ObservableCollection<UserDto> Users { get; } = new();

        [ObservableProperty]
        private TaskDetailDto? _task;

        [ObservableProperty]
        private UserDto? _selectedUser;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        private int? _loadedTaskId;

        public DateOnly? NextActionDate =>
            Comments.Where(c => c.ReminderDate.HasValue).Min(c => c.ReminderDate);

        public async Task LoadAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var detail = await _taskService.GetByIdAsync(id, cancellationToken);

                if (detail is null)
                {
                    Clear();
                    ErrorMessage = "This task no longer exists.";
                    return;
                }

                _loadedTaskId = id;
                Task = detail;

                Comments.Clear();
                foreach (var comment in detail.Comments)
                {
                    Comments.Add(comment);
                }

                OnPropertyChanged(nameof(NextActionDate));
            }
            catch (Exception ex)
            {
                Clear();
                ErrorMessage = $"Could not load the task: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ReloadAsync()
        {
            if (_loadedTaskId is int id)
            {
                await LoadAsync(id);
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

        private void Clear()
        {
            Task = null;
            Comments.Clear();
            _loadedTaskId = null;
            OnPropertyChanged(nameof(NextActionDate));
        }
    }
}
