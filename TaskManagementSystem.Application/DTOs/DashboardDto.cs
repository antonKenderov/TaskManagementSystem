
namespace TaskManagementSystem.Application.DTOs
{
    public class DashboardDto
    {
        public int TotalTasks { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int DueThisWeekCount { get; set; }
        public IReadOnlyList<StatusCountDto> StatusBreakdown { get; set; } = new List<StatusCountDto>();
    }
}
