using System;
using System.Collections.Generic;
using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;

namespace EventBooking.Domain.Entities;

public class Event : AuditableEntity
{
    // Properties rely on FluentValidation for basic input rules
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTimeOffset StartDateTime { get; set; }
    public required DateTimeOffset EndDateTime { get; set; }
    public required string Location { get; set; }
    public required string SpeakerName { get; set; }
    public string? SpeakerBio { get; set; }

    // Encapsulated Seats Management (Private Setters for Safety)
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; } = 0;
    public EventStatus Status { get; private set; } = EventStatus.Draft;

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    public Event() { }

    // ==========================================
    // CUSTOM DOMAIN LOGIC (Business Invariants)
    // ==========================================

    public void InitializeSeats(int totalSeats)
    {
        if (totalSeats <= 0)
            throw new DomainException("Total seats must be greater than zero.");

        if (AvailableSeats > 0 || _reservations.Count > 0)
            throw new DomainException("Seats have already been initialized or reservations exist.");

        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateTimeOffset startDateTime,
        DateTimeOffset endDateTime,
        string location,
        string speakerName,
        string? speakerBio)
    {
        if (Status == EventStatus.Completed || Status == EventStatus.Cancelled)
            throw new DomainException($"Cannot update details for an event in '{Status}' state.");

        Title = title;
        Description = description;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Location = location;
        SpeakerName = speakerName;
        SpeakerBio = speakerBio;
    }

    public void UpdateTotalSeats(int newTotalSeats)
    {
        if (Status == EventStatus.Completed || Status == EventStatus.Cancelled)
            throw new DomainException($"Cannot update capacity for an event in '{Status}' state.");

        if (newTotalSeats <= 0)
            throw new DomainException("Total seats must be greater than zero.");

        if (AvailableSeats == 0 && _reservations.Count == 0)
        {
            TotalSeats = newTotalSeats;
            AvailableSeats = newTotalSeats;
            return;
        }

        int reservedSeats = TotalSeats - AvailableSeats;
        if (newTotalSeats < reservedSeats)
            throw new DomainException("New total capacity cannot be less than the number of already reserved seats.");

        TotalSeats = newTotalSeats;
        AvailableSeats = newTotalSeats - reservedSeats;
    }

    public void Publish(DateTimeOffset currentTime)
    {
        if (Status != EventStatus.Draft)
            throw new DomainException($"Cannot publish an event from '{Status}' state. Event must be Draft.");

        if (currentTime >= StartDateTime)
            throw new DomainException("Cannot publish an event that has already started or passed.");

        if (TotalSeats == 0)
            throw new DomainException("Cannot publish an event before initializing seats.");

        Status = EventStatus.Published;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainException("Event is already cancelled.");

        if (Status == EventStatus.Completed)
            throw new DomainException("Cannot cancel a completed event.");

        Status = EventStatus.Cancelled;
    }

    public void Complete(DateTimeOffset currentTime)
    {
        if (Status != EventStatus.Published)
            throw new DomainException($"Event can only be completed from 'Published' state. Current state: {Status}");

        if (currentTime < EndDateTime)
            throw new DomainException("Cannot complete an event before its scheduled end time.");

        Status = EventStatus.Completed;
    }

    public void ReserveSeats(int numberOfSeats, DateTimeOffset currentTime)
    {
        if (Status != EventStatus.Published)
            throw new DomainException($"Cannot reserve seats unless the event is Published. Current state is {Status}.");

        if (currentTime >= StartDateTime)
            throw new DomainException("Cannot reserve seats after the event has started.");

        if (AvailableSeats < numberOfSeats)
            throw new DomainException("Not enough available seats.");

        AvailableSeats -= numberOfSeats;
    }

    public void ReleaseSeats(int numberOfSeats)
    {
        if (AvailableSeats + numberOfSeats > TotalSeats)
            throw new DomainException("Available seats cannot exceed total seats limit.");

        AvailableSeats += numberOfSeats;
    }
}