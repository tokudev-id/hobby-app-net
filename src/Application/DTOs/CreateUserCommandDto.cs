using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.DTOs;

public class CreateUserCommandDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<HobbyItemDto> Hobbies { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
}

public class HobbyItemDto
{
    public string Name { get; set; } = string.Empty;
    public HobbyLevel Level { get; set; }
}

