
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class CommentSearchResultDto
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public required string TaskDescription { get; set; }
        public required string Text { get; set; }
        public CommentType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateOnly? ReminderDate { get; set; }     
    }
}
