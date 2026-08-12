using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", t =>
        {
            t.HasCheckConstraint("CK_Event_TotalSeats", "[TotalSeats] > 0");
            t.HasCheckConstraint("CK_Event_AvailableSeats", "[AvailableSeats] >= 0 AND [AvailableSeats] <= [TotalSeats]");
            t.HasCheckConstraint("CK_Event_Dates", "[EndDateTime] > [StartDateTime]");
        });

        builder.HasKey(e => e.Id);

        // Max lengths aligned 100% with FluentValidation rules
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(300);
        builder.Property(e => e.SpeakerName).IsRequired().HasMaxLength(150);
        builder.Property(e => e.SpeakerBio).HasMaxLength(1000);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Concurrency
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Indexes
        builder.HasIndex(e => e.StartDateTime);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasMany(e => e.Reservations)
               .WithOne(r => r.Event)
               .HasForeignKey(r => r.EventId)
               .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}