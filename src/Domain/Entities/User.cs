using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Domain.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Hobby> Hobbies { get; set; } = new List<Hobby>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    // For roles assigned by this user
    public ICollection<UserRole> AssignedRoles { get; set; } = new List<UserRole>();
}

