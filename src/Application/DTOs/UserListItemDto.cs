namespace HobbyApp.Application.DTOs;

public class UserListItemDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int HobbyCount { get; set; }
    public List<HobbyItemDto> Hobbies { get; set; } = new();
    public List<RoleDto> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

