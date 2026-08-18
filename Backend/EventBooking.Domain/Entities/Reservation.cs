using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class Reservation : AuditableEntity
{
    public required Guid UserId { get; init; }
    public required Guid EventId { get; init; }
    public required int NumberOfSeats { get; init; }

    public DateTimeOffset BookingDateTime { get; init; } = DateTimeOffset.UtcNow;
    public ReservationStatus Status { get; private set; } = ReservationStatus.Confirmed;

    // Navigation properties
    public User? User { get; private set; }
    public Event? Event { get; private set; }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
            throw new DomainException("Reservation is already cancelled.");

        Status = ReservationStatus.Cancelled;
    }
}