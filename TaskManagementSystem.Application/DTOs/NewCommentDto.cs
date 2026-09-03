
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class NewCommentDto
    {
        public int TaskItemId { get; set; }
        public required string Text { get; set; }
        public CommentType Type { get; set; }
        public DateOnly? ReminderDate { get; set; }
    }
}
