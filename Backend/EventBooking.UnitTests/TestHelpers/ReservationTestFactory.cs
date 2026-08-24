using System.Reflection;
using EventBooking.Domain.Entities;

namespace EventBooking.UnitTests.TestHelpers;

/// <summary>
/// Reservation.Event عندها private setter لأن EF Core هو المسؤول عنها عادةً (relationship
/// fixup وقت الـ Include). في الـ Application-layer unit tests إحنا بنـ mock الـ
/// repositories فمفيش EF حقيقي بيعمل الـ fixup ده، فمحتاجين طريقة نحاكي بيها "الـ
/// reservation ده اتحمل من الـ DB ومعاه الـ Event بتاعه" - وده بالظبط اللي
/// ReservationRepository.GetByIdAsync بيرجعه في الواقع (Include(r => r.Event)).
///
/// استخدام reflection هنا مقصود ومحدود لغرض التيست بس؛ مفيش أي كود إنتاجي بيعمل كده.
/// </summary>
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