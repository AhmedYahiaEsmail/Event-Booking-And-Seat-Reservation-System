using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);

        // Save Enum as String for readability in DB
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Filtered Unique Index: Prevents duplicate emails only for active (non-deleted) users
        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasMany(u => u.Reservations)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Soft Delete Global Query Filter
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
