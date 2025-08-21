using AutoMapper;
using HobbyApp.Application.DTOs;
using HobbyApp.Application.Services.Base.User;
using HobbyApp.Application.Services.Command.User;
using HobbyApp.Application.Services.Query.User;

namespace HobbyApp.Application.Services.Base.User;

public class UserService : IUserService
{
    private readonly IUserCommandService _commandService;
    private readonly IUserQueryService _queryService;

    public UserService(IUserCommandService commandService, IUserQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    public async Task<UserDetailDto?> GetByIdAsync(int id) =>
        await _queryService.GetByIdAsync(id);

    public async Task<PaginatedResult<UserListItemDto>> GetPagedAsync(int page, int size, string? search = null) =>
        await _queryService.GetPagedAsync(page, size, search);

    public async Task<int> CreateAsync(CreateUserCommandDto dto) =>
        await _commandService.CreateAsync(dto);

    public async Task UpdateAsync(UpdateUserCommandDto dto) =>
        await _commandService.UpdateAsync(dto);

    public async Task DeleteAsync(int id) =>
        await _commandService.DeleteAsync(id);

    public async Task<bool> ExistsAsync(int id) =>
        await _queryService.ExistsAsync(id);
}

