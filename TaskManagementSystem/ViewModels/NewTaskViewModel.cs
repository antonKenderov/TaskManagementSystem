using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.ViewModels
{
    /// <summary>
    /// The new task form. Owns its own fields, validation and saving state so the
    /// task list is left with nothing but the list.
    /// </summary>
    public partial class NewTaskViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;

        public NewTaskViewModel(ITaskService taskService, IUserService userService)
        {
            _taskService = taskService;
            _userService = userService;
        }

        /// <summary>
        /// Called after a task is created, so the screen that owns this form can
        /// refresh itself without the form knowing anything about it.
        /// </summary>
        public Func<Task>? TaskCreated { get; set; }

        public ObservableCollection<UserDto> Users { get; } = new();

        public IEnumerable<Status> Statuses => Enum.GetValues<Status>();
        public IEnumerable<TaskType> Types => Enum.GetValues<TaskType>();

        [ObservableProperty]
        private bool _isOpen;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime? _requiredBy;

        [ObservableProperty]
        private Status _selectedStatus = Status.Open;

        [ObservableProperty]
        private TaskType _selectedType = TaskType.FeatureRequest;

        [ObservableProperty]
        private UserDto? _selectedUser;

        /// <summary>
        /// Shown as a preview only. The value the database records is stamped by
        /// SaveChanges, so this is refreshed each time the form opens.
        /// </summary>
        [ObservableProperty]
        private DateTime _createdAtPreview = DateTime.Now;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
        private bool _isSaving;

        [ObservableProperty]
        private string? _errorMessage;

        private bool CanCreate => !IsSaving;

        [RelayCommand]
        private async Task OpenAsync()
        {
            await LoadUsersAsync();

            Reset();
            CreatedAtPreview = DateTime.Now;
            IsOpen = true;
        }

        [RelayCommand]
        private void Close()
        {
            IsOpen = false;
        }

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private async Task CreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Please enter a description.";
                return;
            }

            if (RequiredBy is null)
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

                await _taskService.CreateTaskAsync(new NewTaskDto
                {
                    Description = Description,
                    RequiredByDate = DateOnly.FromDateTime(RequiredBy.Value),
                    Type = SelectedType,
                    Status = SelectedStatus,
                    AssignedToUserId = SelectedUser.Id
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not create the task: {ex.Message}";
                return;
            }
            finally
            {
                IsSaving = false;
            }

            Close();

            if (TaskCreated is not null)
            {
                await TaskCreated();
            }
        }

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

        private void Reset()
        {
            Description = null;
            RequiredBy = null;
            SelectedStatus = Status.Open;
            SelectedType = TaskType.FeatureRequest;
            ErrorMessage = null;
        }
    }
}
