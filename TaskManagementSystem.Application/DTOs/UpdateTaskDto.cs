using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class UpdateTaskDto
    {
        public int Id { get; set; }
        public required string Description { get; set; }
        public DateOnly RequiredByDate { get; set; }
        public TaskType Type { get; set; }
        public Status Status { get; set; }
        public int AssignedToUserId { get; set; }
    }
}
