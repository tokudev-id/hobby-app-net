namespace HobbyApp.Application.Services.Base.Authorization;

public interface IAuthorizationService
{
    Task<bool> IsAdminAsync(int userId);
    Task<bool> CanAccessUserDataAsync(int currentUserId, int targetUserId);
    Task<bool> HasRoleAsync(int userId, string roleName);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
}
