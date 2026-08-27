namespace EventBooking.Application.DTOs.Events;

public record UpdateEventRequest(
    string Title,
    string? Description,
    DateTimeOffset StartDateTime,
    DateTimeOffset EndDateTime,
    string Location,
    string SpeakerName,
    string? SpeakerBio,
    int TotalSeats,
    byte[] RowVersion);