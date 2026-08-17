namespace EventBooking.Application.Exceptions;

public class ReservationAccessDeniedException : Exception
{
    public ReservationAccessDeniedException()
        : base("You do not have permission to access this reservation.")
    {
    }
}