using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Infrastructure.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", t =>
        {
            // Database-Level Constraints for Integrity
            t.HasCheckConstraint("CK_Reservation_NumberOfSeats", "[NumberOfSeats] > 0");
        });
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Indexes for performance (User History & Event Bookings)
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.EventId);

        // Composite indexes added for TASK-04: GetUserReservationsAsync filters by
        // UserId + optional Status, and event-side capacity/reporting queries are
        // expected to filter Reservations by EventId + Status (e.g. counting Confirmed
        // reservations for an event). The existing single-column indexes above remain
        // useful for queries that filter by UserId or EventId alone.
        builder.HasIndex(r => new { r.UserId, r.Status });
        builder.HasIndex(r => new { r.EventId, r.Status });

        // Soft Delete Global Query Filter
        builder.HasQueryFilter(r => !r.IsDeleted);

        // NOTE: Reservation -> User and Reservation -> Event relationships are
        // intentionally NOT configured here. They are already fully configured from the
        // other side, with DeleteBehavior.Restrict (no cascade delete, satisfying the
        // "do not accidentally physically delete reservations" requirement):
        //   UserConfiguration:  builder.HasMany(u => u.Reservations).WithOne(r => r.User)
        //                              .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        //   EventConfiguration: builder.HasMany(e => e.Reservations).WithOne(r => r.Event)
        //                              .HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Restrict);
        // Re-declaring the same relationship here would be redundant and risks a
        // conflicting Fluent API configuration if it ever drifted from those definitions.
    }
}