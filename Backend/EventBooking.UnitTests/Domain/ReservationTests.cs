using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventBooking.UnitTests.Domain;

public class ReservationTests
{
    private static Reservation CreateConfirmedReservation(int numberOfSeats = 2)
    {
        return new Reservation
        {
            UserId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            NumberOfSeats = numberOfSeats
        };
    }

    [Fact]
    public void NewReservation_DefaultsToConfirmedStatus()
    {
        var reservation = CreateConfirmedReservation();

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Cancel_FromConfirmedStatus_SetsStatusToCancelled()
    {
        var reservation = CreateConfirmedReservation();

        reservation.Cancel();

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        // Guards against double-cancel: e.g. the same user double-clicking "Cancel"
        // and firing two sequential cancel requests for the same reservation.
        var reservation = CreateConfirmedReservation();
        reservation.Cancel();

        var act = () => reservation.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("*already cancelled*");
    }
}