using HobbyApp.Infrastructure.Repositories.Interfaces;

namespace HobbyApp.Application.Services.Base.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        return await _unitOfWork.UserRoles.IsUserAdminAsync(userId);
    }

    public async Task<bool> CanAccessUserDataAsync(int currentUserId, int targetUserId)
    {
        // User can access their own data
        if (currentUserId == targetUserId)
            return true;

        // Admin can access any user's data
        return await IsAdminAsync(currentUserId);
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        return await _unitOfWork.UserRoles.UserHasRoleAsync(userId, roleName);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        var roles = await _unitOfWork.UserRoles.GetUserRolesAsync(userId);
        return roles.Select(r => r.Name);
    }
}
