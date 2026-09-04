using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.ViewModels
{
    /// <summary>
    /// Writing side of the comments panel: one form that both posts a new comment
    /// and edits an existing one, plus deleting. The task detail owns the reading
    /// side and refreshes itself through the CommentsChanged callback.
    /// </summary>
    public partial class CommentComposerViewModel : ObservableObject
    {
        private readonly ICommentService _commentService;

        public CommentComposerViewModel(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Raised after a comment is added, edited or removed.
        /// </summary>
        public Func<Task>? CommentsChanged { get; set; }

        /// <summary>
        /// The task comments are written against. Set by the screen that hosts this form.
        /// </summary>
        public int? TaskId { get; set; }

        public IEnumerable<CommentType> CommentTypes => Enum.GetValues<CommentType>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string? _text;

        [ObservableProperty]
        private CommentType _type = CommentType.InternalNote;

        [ObservableProperty]
        private DateTime? _reminder;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _isPosting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEditing))]
        [NotifyPropertyChangedFor(nameof(SubmitLabel))]
        private CommentDto? _editing;

        [ObservableProperty]
        private string? _errorMessage;

        public bool IsEditing => Editing is not null;

        public string SubmitLabel => IsEditing ? "Save comment" : "Post comment";

        private bool CanSave => !IsPosting && !string.IsNullOrWhiteSpace(Text);

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (TaskId is not int taskId || string.IsNullOrWhiteSpace(Text))
            {
                return;
            }

            var reminder = Reminder is null ? (DateOnly?)null : DateOnly.FromDateTime(Reminder.Value);

            try
            {
                IsPosting = true;
                ErrorMessage = null;

                if (Editing is null)
                {
                    await _commentService.CreateCommentAsync(new NewCommentDto
                    {
                        TaskItemId = taskId,
                        Text = Text,
                        Type = Type,
                        ReminderDate = reminder
                    });
                }
                else
                {
                    await _commentService.UpdateCommentAsync(new UpdateCommentDto
                    {
                        Id = Editing.Id,
                        Text = Text,
                        Type = Type,
                        ReminderDate = reminder
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not save the comment: {ex.Message}";
                return;
            }
            finally
            {
                IsPosting = false;
            }

            Reset();
            await NotifyChangedAsync();
        }

        [RelayCommand]
        private void BeginEdit(CommentDto? comment)
        {
            if (comment is null)
            {
                return;
            }

            Editing = comment;
            Text = comment.Text;
            Type = comment.Type;
            Reminder = comment.ReminderDate?.ToDateTime(TimeOnly.MinValue);
            ErrorMessage = null;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            Reset();
        }

        [RelayCommand]
        private async Task DeleteAsync(CommentDto? comment)
        {
            if (comment is null)
            {
                return;
            }

            try
            {
                IsPosting = true;
                ErrorMessage = null;

                await _commentService.DeleteCommentAsync(comment.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not delete the comment: {ex.Message}";
                return;
            }
            finally
            {
                IsPosting = false;
            }

            if (Editing?.Id == comment.Id)
            {
                Reset();
            }

            await NotifyChangedAsync();
        }

        public void Reset()
        {
            Editing = null;
            Text = null;
            Type = CommentType.InternalNote;
            Reminder = null;
            ErrorMessage = null;
        }

        private async Task NotifyChangedAsync()
        {
            if (CommentsChanged is not null)
            {
                await CommentsChanged();
            }
        }
    }
}
