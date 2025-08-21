using HobbyApp.Domain.Entities;

namespace HobbyApp.Infrastructure.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> RoleNameExistsAsync(string name, int? excludeRoleId = null);
}
