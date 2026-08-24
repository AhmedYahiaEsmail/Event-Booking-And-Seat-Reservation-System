using EventBooking.Domain.Entities;
using EventBooking.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace EventBooking.UnitTests.Domain;

/// <summary>
/// دول مش unit tests معزولة لـ entity واحد بس — بيغطوا الـ interaction بين Event و
/// Reservation زي ما بيحصل فعليًا في ReservationService.ReserveSeatsAsync /
/// CancelReservationAsync، لكن بالكامل في الميموري من غير أي DB أو Mocking. الهدف إننا
/// نتأكد إن الـ invariant الأساسي (seats decrement on reserve, increment on cancel)
/// شغال صح على مستوى الـ domain قبل حتى ما نوصل لمرحلة الـ Integration Tests.
/// </summary>
public class ReservationEventInteractionTests
{
    [Fact]
    public void ReserveThenCancel_ReturnsAvailableSeatsToOriginalCount()
    {
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 50);
        var userId = Guid.NewGuid();

        // Mirrors ReservationService.ReserveSeatsAsync
        eventEntity.ReserveSeats(numberOfSeats: 5, EventTestFactory.Now);
        var reservation = new Reservation
        {
            UserId = userId,
            EventId = eventEntity.Id,
            NumberOfSeats = 5
        };

        eventEntity.AvailableSeats.Should().Be(45);

        // Mirrors ReservationService.CancelReservationAsync
        reservation.Cancel();
        eventEntity.ReleaseSeats(reservation.NumberOfSeats);

        eventEntity.AvailableSeats.Should().Be(50);
    }

    [Fact]
    public void TwoReservationsExceedingCapacity_SecondReservationIsRejectedAndFirstIsUnaffected()
    {
        // Sequential simulation of the overbooking scenario (true concurrent races are
        // covered separately by Integration Tests against a real database — see the
        // EventBooking.IntegrationTests project). This proves the domain-level guard
        // itself is correct in isolation: given only 10 seats, a first reservation of 7
        // must succeed, and a second reservation of 5 (7 + 5 = 12 > 10) must be rejected
        // without touching the seats the first reservation already holds.
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 10);

        eventEntity.ReserveSeats(7, EventTestFactory.Now);
        eventEntity.AvailableSeats.Should().Be(3);

        var act = () => eventEntity.ReserveSeats(5, EventTestFactory.Now);

        act.Should().Throw<EventBooking.Domain.Exceptions.DomainException>()
            .WithMessage("*Not enough available seats*");
        eventEntity.AvailableSeats.Should().Be(3, "the rejected reservation must not partially decrement seats");
    }
}