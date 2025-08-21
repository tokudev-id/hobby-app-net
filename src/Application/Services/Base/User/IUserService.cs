using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Services.Base.User;

public interface IUserService
{
    Task<UserDetailDto?> GetByIdAsync(int id);
    Task<PaginatedResult<UserListItemDto>> GetPagedAsync(int page, int size, string? search = null);
    Task<int> CreateAsync(CreateUserCommandDto dto);
    Task UpdateAsync(UpdateUserCommandDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

