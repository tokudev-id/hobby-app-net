using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Persistence;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Infrastructure.Repositories.Implementations;

public class HobbyRepository : IHobbyRepository
{
    private readonly AppDbContext _context;

    public HobbyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Hobby?> GetByIdAsync(int id)
    {
        return await _context.Hobbies
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<Hobby>> GetByUserIdAsync(int userId)
    {
        return await _context.Hobbies
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Hobby> CreateAsync(Hobby hobby)
    {
        await _context.Hobbies.AddAsync(hobby);
        await _context.SaveChangesAsync();
        return hobby;
    }

    public async Task UpdateAsync(Hobby hobby)
    {
        _context.Hobbies.Update(hobby);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var hobby = await _context.Hobbies.FindAsync(id);
        if (hobby != null)
        {
            _context.Hobbies.Remove(hobby);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Hobbies.AnyAsync(h => h.Id == id);
    }

    public async Task<bool> HobbyExistsForUserAsync(int userId, string hobbyName, int? excludeHobbyId = null)
    {
        var query = _context.Hobbies.Where(h =>
            h.UserId == userId &&
            h.Name.ToLower() == hobbyName.ToLower());

        if (excludeHobbyId.HasValue)
        {
            query = query.Where(h => h.Id != excludeHobbyId.Value);
        }

        return await query.AnyAsync();
    }
}

