
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public CommentType Type { get; set; }
        public DateOnly? ReminderDate { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
