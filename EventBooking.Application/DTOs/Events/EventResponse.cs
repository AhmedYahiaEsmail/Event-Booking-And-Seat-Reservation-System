using System;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.DTOs.Events;

public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    string Location,
    string SpeakerName,
    string? SpeakerBio,
    int TotalSeats,
    int AvailableSeats,
    EventStatus Status,
    byte[] RowVersion);