using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class Event : AuditableEntity
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset StartDateTime { get; init; }
    public required DateTimeOffset EndDateTime { get; init; }
    public required string Location { get; init; }
    public required string SpeakerName { get; init; }
    public string? SpeakerBio { get; init; }
    public required int TotalSeats { get; init; }
    public int AvailableSeats { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Draft;

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    public void InitializeSeats()
    {
        AvailableSeats = TotalSeats;
    }

    public void ReserveSeats(int numberOfSeats)
    {
        if (AvailableSeats < numberOfSeats)
            throw new DomainException("Not enough available seats.");

        AvailableSeats -= numberOfSeats;
    }

    public void ReleaseSeats(int numberOfSeats)
    {
        AvailableSeats += numberOfSeats;
    }
}