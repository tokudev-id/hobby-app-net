using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobbyApp.Infrastructure.Persistence.Configurations;

public class HobbyConfiguration : IEntityTypeConfiguration<Hobby>
{
    public void Configure(EntityTypeBuilder<Hobby> builder)
    {
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Level)
            .IsRequired()
            .HasConversion<string>();

        // Remove default value SQL - let EF Core handle defaults in application code
    }
}

