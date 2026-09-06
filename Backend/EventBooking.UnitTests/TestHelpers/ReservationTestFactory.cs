using EventBooking.Domain.Entities;
using System.Reflection;

namespace EventBooking.UnitTests.TestHelpers;

public static class ReservationTestFactory
{
    public static Reservation CreateConfirmedReservation(
        Guid? id = null,
        Guid? userId = null,
        Guid? eventId = null,
        int numberOfSeats = 2,
        Event? attachedEvent = null)
    {
        var reservation = new Reservation
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            EventId = eventId ?? attachedEvent?.Id ?? Guid.NewGuid(),
            NumberOfSeats = numberOfSeats
        };

        if (attachedEvent is not null)
        {
            AttachEvent(reservation, attachedEvent);
        }

        return reservation;
    }

    public static void AttachEvent(Reservation reservation, Event eventEntity)
    {
        var property = typeof(Reservation).GetProperty(nameof(Reservation.Event),
            BindingFlags.Public | BindingFlags.Instance)!;

        property.SetValue(reservation, eventEntity);
    }
}