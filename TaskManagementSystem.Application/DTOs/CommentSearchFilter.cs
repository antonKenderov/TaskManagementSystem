using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class CommentSearchFilter
    {
        public string? Text { get; set; }
        public CommentType? Type { get; set; }
        public DateOnly? ReminderFrom { get; set; }
        public DateOnly? ReminderTo { get; set; }
        public DateTime? AddedFrom { get; set; }
        public DateTime? AddedTo { get; set; }
    }
}
