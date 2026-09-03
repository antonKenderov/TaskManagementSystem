using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class TaskFilter
    {
        public Status? Status { get; set; }
        public TaskType? Type { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
