using HobbyApp.Domain.Entities;

namespace HobbyApp.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetPagedAsync(int page, int size, string? search = null);
    Task<int> GetTotalCountAsync(string? search = null);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
}

