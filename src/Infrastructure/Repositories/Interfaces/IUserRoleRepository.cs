using HobbyApp.Domain.Entities;

namespace HobbyApp.Infrastructure.Repositories.Interfaces;

public interface IUserRoleRepository
{
    Task<UserRole?> GetByIdAsync(int id);
    Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);
    Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId);
    Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
    Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId);
    Task<UserRole> CreateAsync(UserRole userRole);
    Task<UserRole> AddAsync(UserRole userRole);
    Task<IEnumerable<UserRole>> GetAllAsync();
    Task DeleteAsync(int id);
    Task DeleteByUserAndRoleAsync(int userId, int roleId);
    Task<bool> UserHasRoleAsync(int userId, int roleId);
    Task<bool> UserHasRoleAsync(int userId, string roleName);
    Task<bool> IsUserAdminAsync(int userId);
}
