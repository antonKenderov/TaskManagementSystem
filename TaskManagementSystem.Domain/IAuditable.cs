namespace TaskManagementSystem.Domain
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
        string? ModifiedBy { get; set; }
    }
}
