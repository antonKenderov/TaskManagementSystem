using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Enums;

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

        public IEnumerable<Status> Statuses => Enum.GetValues<Status>();
        public IEnumerable<TaskType> Types => Enum.GetValues<TaskType>();

        [ObservableProperty]
        private TaskDetailDto? _task;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private string? _description;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private DateTime? _requiredByDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private Status _selectedStatus;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private TaskType _selectedType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private UserDto? _selectedUser;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
        private bool _isSaving;

        [ObservableProperty]
        private string? _errorMessage;

        private int? _loadedTaskId;

        public string FormattedTaskId => Task is null ? string.Empty : $"TSK-{Task.Id:D4}";

        public DateOnly? NextActionDate =>
            Comments.Where(c => c.ReminderDate.HasValue).Min(c => c.ReminderDate);

        public bool HasChanges =>
            Task is not null &&
            (Description != Task.Description
             || RequiredByDate?.Date != Task.RequiredBy.ToDateTime(TimeOnly.MinValue)
             || SelectedStatus != Task.Status
             || SelectedType != Task.Type
             || SelectedUser?.Id != Task.AssignedToUserId);

        private bool CanSaveChanges => !IsSaving && HasChanges;

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

                await LoadUsersAsync(cancellationToken);
                ResetEditableFields();
                NotifyDerived();
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

        [RelayCommand(CanExecute = nameof(CanSaveChanges))]
        private async Task SaveChangesAsync()
        {
            if (Task is null || SelectedUser is null || RequiredByDate is null)
            {
                ErrorMessage = "Please fill in every field before saving.";
                return;
            }

            var taskId = Task.Id;

            try
            {
                IsSaving = true;
                ErrorMessage = null;

                await _taskService.UpdateTaskAsync(new UpdateTaskDto
                {
                    Id = taskId,
                    Description = Description ?? string.Empty,
                    RequiredByDate = DateOnly.FromDateTime(RequiredByDate.Value),
                    Status = SelectedStatus,
                    Type = SelectedType,
                    AssignedToUserId = SelectedUser.Id
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not save the task: {ex.Message}";
                return;
            }
            finally
            {
                IsSaving = false;
            }

            await LoadAsync(taskId);
        }

        [RelayCommand]
        private void CancelChanges()
        {
            ResetEditableFields();
            ErrorMessage = null;
        }

        [RelayCommand]
        private async Task ReloadAsync()
        {
            if (_loadedTaskId is int id)
            {
                await LoadAsync(id);
            }
        }

        private async Task LoadUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetAllAsync(cancellationToken);

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private void ResetEditableFields()
        {
            Description = Task?.Description;
            RequiredByDate = Task?.RequiredBy.ToDateTime(TimeOnly.MinValue);
            SelectedStatus = Task?.Status ?? Status.Open;
            SelectedType = Task?.Type ?? TaskType.FeatureRequest;
            SelectedUser = Users.FirstOrDefault(u => u.Id == Task?.AssignedToUserId);

            OnPropertyChanged(nameof(HasChanges));
            SaveChangesCommand.NotifyCanExecuteChanged();
        }

        private void Clear()
        {
            Task = null;
            Comments.Clear();
            Users.Clear();
            _loadedTaskId = null;
            ResetEditableFields();
            NotifyDerived();
        }

        private void NotifyDerived()
        {
            OnPropertyChanged(nameof(FormattedTaskId));
            OnPropertyChanged(nameof(NextActionDate));
            OnPropertyChanged(nameof(HasChanges));
        }
    }
}
