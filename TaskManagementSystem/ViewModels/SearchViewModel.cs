using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;

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

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private bool _isSearching;

        [ObservableProperty]
        private string? _errorMessage;

        public bool ShowingRecent => string.IsNullOrWhiteSpace(SearchText);

        partial void OnSearchTextChanged(string? value)
        {
            OnPropertyChanged(nameof(ShowingRecent));
            SearchCommand.Execute(null);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var text = SearchText;

            try
            {
                IsSearching = true;
                ErrorMessage = null;

                var matches = await _commentService.SearchCommentsAsync(new CommentSearchFilter
                {
                    Text = string.IsNullOrWhiteSpace(text) ? null : text
                });

                if (string.IsNullOrWhiteSpace(text))
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
        private async Task OpenTaskAsync(CommentSearchResultDto? result)
        {
            if (result is null || OpenTask is null)
            {
                return;
            }

            await OpenTask(result.TaskItemId);
        }
    }
}
