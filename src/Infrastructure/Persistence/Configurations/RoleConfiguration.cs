using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobbyApp.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        // Index for unique role names
        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Seed default roles
        builder.HasData(
            new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "Administrator with full access to manage users and system settings",
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = 2,
                Name = "User",
                Description = "Regular user who can manage their own profile and hobbies",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
