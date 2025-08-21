using HobbyApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HobbyApp.Infrastructure.Persistence;

public class DatabaseSeederService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeederService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DatabaseSeederService(AppDbContext context, ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if users already exist
            var existingUserCount = await _context.Users.CountAsync();
            _logger.LogInformation($"Found {existingUserCount} existing users in database");

            if (existingUserCount > 0)
            {
                _logger.LogInformation("Users already exist. Checking if they have role assignments...");

                // Check if users have role assignments
                var userRoleCount = await _context.UserRoles.CountAsync();
                _logger.LogInformation($"Found {userRoleCount} user-role assignments");

                if (userRoleCount > 0)
                {
                    _logger.LogInformation("Role assignments already exist. Skipping seeding.");
                    return;
                }
                else
                {
                    _logger.LogInformation("Users exist but no role assignments found. Assigning roles to existing users...");

                    // Get all existing users and assign roles
                    var existingUsers = await _context.Users.ToListAsync();
                    await SeedUserRolesAsync(existingUsers);
                    return;
                }
            }

            // Create default users
            var defaultUsers = new List<User>
            {
                new User
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = "admin@hobbyapp.com",
                    PasswordHash = _passwordHasher.HashPassword(new User(), "Admin123!"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Hobbies = new List<Hobby>
                    {
                        new Hobby
                        {
                            Name = "Reading",
                            Level = HobbyLevel.Expert,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Hobby
                        {
                            Name = "Coding",
                            Level = HobbyLevel.Intermediate,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                },
                new User
                {
                    Username = "joko_santoso",
                    FullName = "Joko Santoso",
                    Email = "joko.santoso@example.com",
                    PasswordHash = _passwordHasher.HashPassword(new User(), "Password123!"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Hobbies = new List<Hobby>
                    {
                        new Hobby
                        {
                            Name = "Photography",
                            Level = HobbyLevel.Beginner,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Hobby
                        {
                            Name = "Hiking",
                            Level = HobbyLevel.Intermediate,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                },
                new User
                {
                    Username = "asep_wijaya",
                    FullName = "Asep Wijaya",
                    Email = "asep.wijaya@example.com",
                    PasswordHash = _passwordHasher.HashPassword(new User(), "Password123!"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Hobbies = new List<Hobby>
                    {
                        new Hobby
                        {
                            Name = "Painting",
                            Level = HobbyLevel.Expert,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                }
            };

            // Add users to context
            await _context.Users.AddRangeAsync(defaultUsers);

            // Save changes to get user IDs
            await _context.SaveChangesAsync();

            // Assign roles to users
            await SeedUserRolesAsync(defaultUsers);

            _logger.LogInformation($"Successfully seeded {defaultUsers.Count} users with their hobbies and roles.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedUserRolesAsync(List<User> users)
    {
        try
        {
            _logger.LogInformation("Starting role assignment for users...");

            // Get roles from database (seeded via migration)
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (adminRole == null)
            {
                _logger.LogError("Admin role not found in database. Cannot proceed with role assignment.");
                return;
            }

            if (userRole == null)
            {
                _logger.LogError("User role not found in database. Cannot proceed with role assignment.");
                return;
            }

            _logger.LogInformation($"Found roles: Admin (ID: {adminRole.Id}), User (ID: {userRole.Id})");

            var userRoles = new List<UserRole>();
            var adminUser = users.FirstOrDefault(u => u.Username == "admin");

            // Assign admin role to admin user
            if (adminUser != null)
            {
                var adminUserRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = null // System assignment
                };
                userRoles.Add(adminUserRole);
                _logger.LogInformation($"Assigned Admin role to user: {adminUser.Username} (ID: {adminUser.Id})");
            }
            else
            {
                _logger.LogWarning("Admin user not found in the user list!");
            }

            // Assign user role to all users
            foreach (var user in users)
            {
                // Check if this user already has the user role (in case of admin user)
                var existingUserRole = userRoles.FirstOrDefault(ur =>
                    ur.UserId == user.Id && ur.RoleId == userRole.Id);

                if (existingUserRole == null)
                {
                    var userRoleAssignment = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = userRole.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = adminUser?.Id // Assigned by admin
                    };
                    userRoles.Add(userRoleAssignment);
                    _logger.LogInformation($"Assigned User role to user: {user.Username} (ID: {user.Id})");
                }
            }

            // Bulk insert user roles
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully assigned roles to {users.Count} users ({userRoles.Count} total role assignments)");

            // Verify role assignments
            var totalUserRoles = await _context.UserRoles.CountAsync();
            _logger.LogInformation($"Total user-role relationships in database: {totalUserRoles}");

            // Log role distribution
            var adminCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id);
            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == userRole.Id);
            _logger.LogInformation($"Role distribution - Admin: {adminCount}, User: {userCount}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding user roles.");
            throw;
        }
    }
}

