using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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

        // Soft Delete Global Query Filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
