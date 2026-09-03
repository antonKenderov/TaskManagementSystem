
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class TaskDetailDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateOnly RequiredBy { get; set; }
        public required string Description { get; set; }

        public Status Status { get; set; }
        public TaskType Type { get; set; }
        public int AssignedToUserId { get; set; }
        public UserDto? AssignedTo { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateOnly? NextActionDate { get; set; }
        public IReadOnlyList<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }
}
