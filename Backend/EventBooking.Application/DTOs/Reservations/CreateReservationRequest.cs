using System;

namespace EventBooking.Application.DTOs.Reservations;

public record CreateReservationRequest(
    Guid EventId,
    int NumberOfSeats);