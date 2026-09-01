using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Domain
{
    public class TaskItem
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateOnly RequiredByDate { get; set; }
        public required string Description { get; set; }
        public Status Status { get; set; }
        public TaskType Type { get; set; }
        public int AssignedToUserId { get; set; }
        public User? AssignedTo { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
