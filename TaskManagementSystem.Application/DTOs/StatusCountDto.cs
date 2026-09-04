
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public class StatusCountDto
    {
        public Status Status { get; set; }
        public int Count { get; set; }
    }
}
