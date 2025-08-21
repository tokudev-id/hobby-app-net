using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Persistence;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Infrastructure.Repositories.Implementations;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _context;

    public UserRoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole?> GetByIdAsync(int id)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Include(ur => ur.AssignedByUser)
            .FirstOrDefaultAsync(ur => ur.Id == id);
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.AssignedByUser)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.AssignedByUser)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId)
    {
        return await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User)
            .ToListAsync();
    }

    public async Task<UserRole> CreateAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();
        return userRole;
    }

    public async Task<UserRole> AddAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
        return userRole;
    }

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Include(ur => ur.AssignedByUser)
            .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var userRole = await _context.UserRoles.FindAsync(id);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserAndRoleAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> UserHasRoleAsync(int userId, int roleId)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        return await UserHasRoleAsync(userId, "Admin");
    }
}
