using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<TaskManagerDbContext> _dbContextFactory;

        public UserService(IDbContextFactory<TaskManagerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await db.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .Select(u => new UserDto(u.Id, u.Name))
                .ToListAsync(cancellationToken);
        }
    }
}
