using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Services.Query.User;

public interface IUserQueryService
{
    Task<UserDetailDto?> GetByIdAsync(int id);
    Task<PaginatedResult<UserListItemDto>> GetPagedAsync(int page, int size, string? search = null);
    Task<bool> ExistsAsync(int id);
}

