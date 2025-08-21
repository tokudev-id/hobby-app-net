using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyApp.Domain.Entities;

public class Hobby
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public HobbyLevel Level { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for many-to-one relationship
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

public enum HobbyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Expert = 3
}

