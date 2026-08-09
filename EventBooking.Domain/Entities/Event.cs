using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class Event : AuditableEntity
{
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset StartDateTime { get; private set; }
    public DateTimeOffset EndDateTime { get; private set; }
    public string Location { get; private set; }
    public string SpeakerName { get; private set; }
    public string? SpeakerBio { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public EventStatus Status { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Event() { }

    public Event(Guid id, string title, string? description, DateTimeOffset startDateTime, DateTimeOffset endDateTime, string location, string speakerName, string? speakerBio, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title cannot be empty.");
        if (totalSeats <= 0) throw new DomainException("Total seats must be greater than zero.");
        if (endDateTime <= startDateTime) throw new DomainException("End date and time must be after start date and time.");

        Id = id;
        Title = title;
        Description = description;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Location = location;
        SpeakerName = speakerName;
        SpeakerBio = speakerBio;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        Status = EventStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Domain behaviors to protect invariants around seats and status
    public void ReserveSeats(int numberOfSeats)
    {
        if (numberOfSeats <= 0) throw new DomainException("Must reserve at least one seat.");
        if (AvailableSeats < numberOfSeats) throw new DomainException("Not enough available seats.");

        AvailableSeats -= numberOfSeats;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReleaseSeats(int numberOfSeats)
    {
        if (numberOfSeats <= 0) throw new DomainException("Must release at least one seat.");
        if (AvailableSeats + numberOfSeats > TotalSeats) throw new DomainException("Available seats cannot exceed total seats.");

        AvailableSeats += numberOfSeats;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}