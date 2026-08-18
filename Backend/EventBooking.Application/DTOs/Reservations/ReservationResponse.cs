using System;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.DTOs.Reservations;

public record ReservationResponse(
    Guid Id,
    Guid EventId,
    string EventTitle,
    DateTimeOffset EventStartDateTime,
    string EventLocation,
    Guid UserId,
    int NumberOfSeats,
    DateTimeOffset BookingDateTime,
    ReservationStatus Status);