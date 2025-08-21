using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.DTOs;

public class UpdateUserCommandDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<HobbyItemDto> Hobbies { get; set; } = new();
}

