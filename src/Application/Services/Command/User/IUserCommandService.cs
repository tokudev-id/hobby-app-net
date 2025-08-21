using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Services.Command.User;

public interface IUserCommandService
{
    Task<int> CreateAsync(CreateUserCommandDto dto);
    Task UpdateAsync(UpdateUserCommandDto dto);
    Task DeleteAsync(int id);
}

