using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", t =>
        {
            // Database-Level Constraints for Integrity
            t.HasCheckConstraint("CK_Event_TotalSeats", "[TotalSeats] > 0");
            t.HasCheckConstraint("CK_Event_AvailableSeats", "[AvailableSeats] >= 0 AND [AvailableSeats] <= [TotalSeats]");
            t.HasCheckConstraint("CK_Event_Dates", "[EndDateTime] > [StartDateTime]");
        });
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.SpeakerName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SpeakerBio).HasMaxLength(500);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Optimistic Concurrency Mechanism (Infrastructure only - Shadow Property)
        // This keeps the Domain pure without needing a byte[] RowVersion in the entity.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Indexes for Event Discovery
        builder.HasIndex(e => e.StartDateTime);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasMany(e => e.Reservations)
               .WithOne(r => r.Event)
               .HasForeignKey(r => r.EventId)
               .OnDelete(DeleteBehavior.Restrict);

        // Soft Delete Global Query Filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
