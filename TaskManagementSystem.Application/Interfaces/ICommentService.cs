
using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces
{
    public interface ICommentService
    {
        Task<int> CreateCommentAsync(NewCommentDto comment, CancellationToken cancellationToken = default);
        Task<int> DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default);
    }
}
