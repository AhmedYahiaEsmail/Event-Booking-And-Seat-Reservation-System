namespace EventBooking.Application.Exceptions;

public class ReservationNotFoundException : Exception
{
    public ReservationNotFoundException(Guid reservationId)
        : base($"Reservation with ID '{reservationId}' was not found.")
    {
    }
}