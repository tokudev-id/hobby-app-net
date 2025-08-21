namespace HobbyApp.Application.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class UserRoleDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = new();
}
