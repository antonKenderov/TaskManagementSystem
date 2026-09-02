
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Domain
{
    public class Comment : IAuditable
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public required string Text { get; set; }
        public CommentType Type { get; set; }
        public DateOnly? ReminderDate { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
