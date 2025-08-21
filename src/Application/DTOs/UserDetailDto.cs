using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.DTOs;

public class UserDetailDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<HobbyItemDto> Hobbies { get; set; } = new();
}

