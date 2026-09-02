using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Application.DTOs
{
    public record TaskTableItemDto(
        int Id,
        DateTime CreatedAt,
        DateOnly RequiredBy,
        string Description,
        TaskType Type,
        Status Status,
        string AssignedToName,
        DateOnly? NextActionDate);
}
