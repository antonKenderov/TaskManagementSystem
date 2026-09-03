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

        public TasksViewModel(
            ITaskService taskService,
            IUserService userService,
            TaskDetailViewModel taskDetail)
        {
            _taskService = taskService;
            _userService = userService;
            TaskDetail = taskDetail;
        }

        public TaskDetailViewModel TaskDetail { get; }

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
        private bool _isDetailsVisible;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadTasksCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isPopupOpen;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string? _newTaskDescription;

        [ObservableProperty]
        private DateTime? _newTaskRequiredBy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateTaskCommand))]
        private bool _isSaving;

        private bool CanLoadTasks => !IsLoading;

        private bool CanCreateTask => !IsSaving;

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

        [RelayCommand(CanExecute = nameof(CanCreateTask))]
        private async Task CreateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskDescription))
            {
                ErrorMessage = "Please enter a description.";
                return;
            }

            if (NewTaskRequiredBy is null)
            {
                ErrorMessage = "Please pick a required by date.";
                return;
            }

            if (SelectedUser is null)
            {
                ErrorMessage = "Please select a user to assign the task to.";
                return;
            }

            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var newTask = new NewTaskDto
                {
                    Description = NewTaskDescription,
                    RequiredByDate = DateOnly.FromDateTime(NewTaskRequiredBy.Value),
                    Type = SelectedType,
                    Status = SelectedStatus,
                    AssignedToUserId = SelectedUser.Id
                };

                await _taskService.CreateTaskAsync(newTask);

                ClosePopup();
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not create the task: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task OpenTaskDetailAsync()
        {
            if (SelectedTask is null)
            {
                return;
            }

            await TaskDetail.LoadAsync(SelectedTask.Id);
            IsDetailsVisible = true;
        }

        [RelayCommand]
        private async Task CloseDetailAsync()
        {
            IsDetailsVisible = false;
            await LoadTasksAsync();
        }

        [RelayCommand]
        private async Task OpenPopupAsync()
        {
            // The assignee list is only needed by the form, so it is fetched when
            // the form opens rather than kept in step with the task list.
            await LoadUsersAsync();

            ResetNewTaskForm();
            IsPopupOpen = true;
        }

        [RelayCommand]
        private void ClosePopup()
        {
            IsPopupOpen = false;
        }

        private void ResetNewTaskForm()
        {
            NewTaskDescription = null;
            NewTaskRequiredBy = null;
            SelectedStatus = Status.Open;
            SelectedType = TaskType.FeatureRequest;
            ErrorMessage = null;
        }
    }
}
