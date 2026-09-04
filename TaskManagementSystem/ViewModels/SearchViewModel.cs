using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private const int RecentCommentCount = 4;

        private readonly ICommentService _commentService;

        public SearchViewModel(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public Func<int, Task>? OpenTask { get; set; }

        public ObservableCollection<CommentSearchResultDto> Results { get; } = new();

        public IEnumerable<FilterOption> TypeFilterOptions =>
            new[] { new FilterOption("Any type", null) }
                .Concat(Enum.GetValues<CommentType>().Select(v => new FilterOption(v.ToString(), v)));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private string? _searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private CommentType? _filterType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private DateTime? _addedFrom;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private DateTime? _addedTo;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private DateTime? _reminderFrom;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowingRecent))]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
        private DateTime? _reminderTo;

        [ObservableProperty]
        private bool _isSearching;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchText)
            || FilterType is not null
            || AddedFrom is not null
            || AddedTo is not null
            || ReminderFrom is not null
            || ReminderTo is not null;

        public bool ShowingRecent => !HasActiveFilters;

        partial void OnSearchTextChanged(string? value) => RunSearch();

        partial void OnFilterTypeChanged(CommentType? value) => RunSearch();

        partial void OnAddedFromChanged(DateTime? value) => RunSearch();

        partial void OnAddedToChanged(DateTime? value) => RunSearch();

        partial void OnReminderFromChanged(DateTime? value) => RunSearch();

        partial void OnReminderToChanged(DateTime? value) => RunSearch();

        private void RunSearch()
        {
            if (_suppressSearch)
            {
                return;
            }

            SearchCommand.Execute(null);
        }

        private bool _suppressSearch;

        [RelayCommand]
        private async Task SearchAsync()
        {
            var showingRecent = ShowingRecent;

            try
            {
                IsSearching = true;
                ErrorMessage = null;

                var matches = await _commentService.SearchCommentsAsync(new CommentSearchFilter
                {
                    Text = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                    Type = FilterType,
                    AddedFrom = ToUtcDayStart(AddedFrom),
                    AddedTo = ToUtcDayEnd(AddedTo),
                    ReminderFrom = AsDateOnly(ReminderFrom),
                    ReminderTo = AsDateOnly(ReminderTo)
                });

                if (showingRecent)
                {
                    matches = matches.Take(RecentCommentCount).ToList();
                }

                Results.Clear();
                foreach (var match in matches)
                {
                    Results.Add(match);
                }
            }
            catch (Exception ex)
            {
                Results.Clear();
                ErrorMessage = $"Could not search the comments: {ex.Message}";
            }
            finally
            {
                IsSearching = false;
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            _suppressSearch = true;
            SearchText = null;
            FilterType = null;
            AddedFrom = null;
            AddedTo = null;
            ReminderFrom = null;
            ReminderTo = null;
            _suppressSearch = false;

            RunSearch();
        }

        [RelayCommand]
        private async Task OpenTaskAsync(CommentSearchResultDto? result)
        {
            if (result is null || OpenTask is null)
            {
                return;
            }

            await OpenTask(result.TaskItemId);
        }

        private static DateOnly? AsDateOnly(DateTime? value) =>
            value is null ? null : DateOnly.FromDateTime(value.Value);

        /// <summary>
        /// The picker hands back a local calendar day with an unspecified kind, while
        /// CreatedAt is stored as UTC, so the day is anchored locally and converted.
        /// </summary>
        private static DateTime? ToUtcDayStart(DateTime? value) =>
            value is null
                ? null
                : DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Local).ToUniversalTime();

        private static DateTime? ToUtcDayEnd(DateTime? value) =>
            value is null
                ? null
                : DateTime.SpecifyKind(value.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local)
                    .ToUniversalTime();
    }
}
