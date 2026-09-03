
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class UpdateCommentDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public CommentType Type { get; set; }
        public DateOnly? ReminderDate { get; set; }
    }
}
