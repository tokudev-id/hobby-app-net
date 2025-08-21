using HobbyApp.Domain.Entities;

namespace HobbyApp.Infrastructure.Repositories.Interfaces;

public interface IHobbyRepository
{
    Task<Hobby?> GetByIdAsync(int id);
    Task<IEnumerable<Hobby>> GetByUserIdAsync(int userId);
    Task<Hobby> CreateAsync(Hobby hobby);
    Task UpdateAsync(Hobby hobby);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> HobbyExistsForUserAsync(int userId, string hobbyName, int? excludeHobbyId = null);
}

