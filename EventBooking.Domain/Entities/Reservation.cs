using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class Reservation : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public int NumberOfSeats { get; private set; }
    public DateTimeOffset BookingDateTime { get; private set; }
    public ReservationStatus Status { get; private set; }

    // Navigation properties for domain relationships
    public User? User { get; private set; }
    public Event? Event { get; private set; }

    private Reservation() { }

    public Reservation(Guid id, Guid userId, Guid eventId, int numberOfSeats)
    {
        if (userId == Guid.Empty) throw new DomainException("User ID cannot be empty.");
        if (eventId == Guid.Empty) throw new DomainException("Event ID cannot be empty.");
        if (numberOfSeats <= 0) throw new DomainException("Number of seats must be greater than zero.");

        Id = id;
        UserId = userId;
        EventId = eventId;
        NumberOfSeats = numberOfSeats;
        BookingDateTime = DateTimeOffset.UtcNow;
        Status = ReservationStatus.Confirmed;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
            throw new DomainException("Reservation is already cancelled.");

        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}