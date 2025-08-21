using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Persistence;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Infrastructure.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .ThenInclude(ur => ur.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Roles.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> RoleNameExistsAsync(string name, int? excludeRoleId = null)
    {
        var query = _context.Roles.Where(r => r.Name == name);
        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }
        return await query.AnyAsync();
    }
}
