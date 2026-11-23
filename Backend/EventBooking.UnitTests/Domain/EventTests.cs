using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using EventBooking.UnitTests.TestHelpers;
using FluentAssertions;

namespace EventBooking.UnitTests.Domain;

public class EventTests
{
    private static readonly DateTimeOffset Now = EventTestFactory.Now;

    // ==========================================
    // InitializeSeats
    // ==========================================

    [Fact]
    public void InitializeSeats_WithValidTotalSeats_SetsTotalAndAvailableSeats()
    {
        var eventEntity = EventTestFactory.CreateEmptyDraftEvent();

        eventEntity.InitializeSeats(100);

        eventEntity.TotalSeats.Should().Be(100);
        eventEntity.AvailableSeats.Should().Be(100);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void InitializeSeats_WithNonPositiveTotalSeats_ThrowsDomainException(int totalSeats)
    {
        var eventEntity = EventTestFactory.CreateEmptyDraftEvent();

        var act = () => eventEntity.InitializeSeats(totalSeats);

        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void InitializeSeats_WhenAlreadyInitialized_ThrowsDomainException()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 50);

        var act = () => eventEntity.InitializeSeats(100);

        act.Should().Throw<DomainException>()
            .WithMessage("*already been initialized*");
    }

    // ==========================================
    // UpdateTotalSeats
    // ==========================================

    [Fact]
    public void UpdateTotalSeats_WhenNoReservationsExist_UpdatesTotalAndAvailableToSameValue()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 50);

        eventEntity.UpdateTotalSeats(200);

        eventEntity.TotalSeats.Should().Be(200);
        eventEntity.AvailableSeats.Should().Be(200);
    }

    [Fact]
    public void UpdateTotalSeats_WhenSeatsAlreadyReserved_RecalculatesAvailableSeatsCorrectly()
    {
        // 100 total, 30 reserved (70 available) -> bump total to 150
        // reservedSeats stays 30, so available should become 150 - 30 = 120
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 30);

        eventEntity.UpdateTotalSeats(150);

        eventEntity.TotalSeats.Should().Be(150);
        eventEntity.AvailableSeats.Should().Be(120);
    }

    [Fact]
    public void UpdateTotalSeats_WhenNewTotalIsLessThanAlreadyReservedSeats_ThrowsDomainException()
    {
        // 100 total, 40 reserved -> trying to shrink to 30 (less than the 40 already booked) must fail
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 40);

        var act = () => eventEntity.UpdateTotalSeats(30);

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be less than the number of already reserved seats*");
    }

    [Fact]
    public void UpdateTotalSeats_WhenNewTotalExactlyEqualsReservedSeats_SucceedsWithZeroAvailable()
    {
        // Boundary case: shrinking exactly down to the reserved count should be allowed (0 available left)
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 40);

        eventEntity.UpdateTotalSeats(40);

        eventEntity.TotalSeats.Should().Be(40);
        eventEntity.AvailableSeats.Should().Be(0);
    }

    [Theory]
    [InlineData(EventStatus.Completed)]
    [InlineData(EventStatus.Cancelled)]
    public void UpdateTotalSeats_WhenEventIsCompletedOrCancelled_ThrowsDomainException(EventStatus status)
    {
        var eventEntity = BuildEventInStatus(status);

        var act = () => eventEntity.UpdateTotalSeats(500);

        act.Should().Throw<DomainException>()
            .WithMessage($"*capacity for an event in '{status}' state*");
    }

    // ==========================================
    // Publish
    // ==========================================

    [Fact]
    public void Publish_FromDraftWithFutureStartAndInitializedSeats_SetsStatusToPublished()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 10, startDateTime: Now.AddDays(1));

        eventEntity.Publish(Now);

        eventEntity.Status.Should().Be(EventStatus.Published);
    }

    [Theory]
    [InlineData(EventStatus.Published)]
    [InlineData(EventStatus.Cancelled)]
    [InlineData(EventStatus.Completed)]
    public void Publish_WhenNotInDraftState_ThrowsDomainException(EventStatus status)
    {
        var eventEntity = BuildEventInStatus(status);

        var act = () => eventEntity.Publish(Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*must be Draft*");
    }

    [Fact]
    public void Publish_WhenStartDateTimeHasAlreadyPassed_ThrowsDomainException()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(
            totalSeats: 10,
            startDateTime: Now.AddDays(-1),
            endDateTime: Now.AddHours(1));

        var act = () => eventEntity.Publish(Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*already started or passed*");
    }

    [Fact]
    public void Publish_WhenSeatsWereNeverInitialized_ThrowsDomainException()
    {
        var eventEntity = EventTestFactory.CreateEmptyDraftEvent(startDateTime: Now.AddDays(1));

        var act = () => eventEntity.Publish(Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*before initializing seats*");
    }

    // ==========================================
    // Cancel
    // ==========================================

    [Fact]
    public void Cancel_FromPublishedState_SetsStatusToCancelled()
    {
        var eventEntity = EventTestFactory.CreatePublishedEvent();

        eventEntity.Cancel();

        eventEntity.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var eventEntity = EventTestFactory.CreatePublishedEvent();
        eventEntity.Cancel();

        var act = () => eventEntity.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public void Cancel_WhenCompleted_ThrowsDomainException()
    {
        var eventEntity = BuildEventInStatus(EventStatus.Completed);

        var act = () => eventEntity.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot cancel a completed event*");
    }

    // ==========================================
    // Complete
    // ==========================================

    [Fact]
    public void Complete_WhenPublishedAndPastEndDateTime_SetsStatusToCompleted()
    {
        var start = Now.AddDays(-2);
        var end = Now.AddDays(-1);
        var eventEntity = EventTestFactory.CreatePublishedEvent(
            startDateTime: start,
            endDateTime: end,
            publishTime: start.AddDays(-10));

        eventEntity.Complete(Now);

        eventEntity.Status.Should().Be(EventStatus.Completed);
    }

    [Theory]
    [InlineData(EventStatus.Draft)]
    [InlineData(EventStatus.Cancelled)]
    [InlineData(EventStatus.Completed)]
    public void Complete_WhenNotPublished_ThrowsDomainException(EventStatus status)
    {
        var eventEntity = BuildEventInStatus(status);

        var act = () => eventEntity.Complete(Now.AddDays(10));

        act.Should().Throw<DomainException>()
            .WithMessage("*can only be completed from 'Published' state*");
    }

    [Fact]
    public void Complete_BeforeScheduledEndTime_ThrowsDomainException()
    {
        var start = Now.AddDays(-1);
        var end = Now.AddDays(1); // still hasn't ended
        var eventEntity = EventTestFactory.CreatePublishedEvent(startDateTime: start, endDateTime: end, publishTime: start.AddDays(-5));

        var act = () => eventEntity.Complete(Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*before its scheduled end time*");
    }

    // ==========================================
    // ReserveSeats — THE most business-critical method: this is what
    // prevents overbooking at the domain level, before concurrency/DB even enters the picture.
    // ==========================================

    [Fact]
    public void ReserveSeats_WithEnoughAvailableSeats_DecrementsAvailableSeats()
    {
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 100);

        eventEntity.ReserveSeats(30, Now);

        eventEntity.AvailableSeats.Should().Be(70);
    }

    [Fact]
    public void ReserveSeats_RequestingExactlyTheLastRemainingSeats_SucceedsAndLeavesZeroAvailable()
    {
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 95);

        eventEntity.ReserveSeats(5, Now);

        eventEntity.AvailableSeats.Should().Be(0);
    }

    [Fact]
    public void ReserveSeats_WhenNotEnoughAvailableSeats_ThrowsDomainExceptionAndLeavesAvailableSeatsUnchanged()
    {
        // Core overbooking-prevention assertion: only 5 left, someone asks for 6 -> must fail,
        // and AvailableSeats must remain untouched (no partial decrement).
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 95);

        var act = () => eventEntity.ReserveSeats(6, Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*Not enough available seats*");
        eventEntity.AvailableSeats.Should().Be(5);
    }

    [Fact]
    public void ReserveSeats_WhenEventIsNotPublished_ThrowsDomainException()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 10);

        var act = () => eventEntity.ReserveSeats(1, Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot reserve seats unless the event is Published*");
    }

    [Fact]
    public void ReserveSeats_AfterEventHasStarted_ThrowsDomainException()
    {
        var start = Now.AddHours(-1);
        var eventEntity = EventTestFactory.CreatePublishedEvent(
            totalSeats: 10,
            startDateTime: start,
            endDateTime: start.AddHours(3),
            publishTime: start.AddDays(-1));

        var act = () => eventEntity.ReserveSeats(1, Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*after the event has started*");
    }

    // ==========================================
    // ReleaseSeats (used by reservation cancellation)
    // ==========================================

    [Fact]
    public void ReleaseSeats_AfterAPriorReservation_IncrementsAvailableSeatsBack()
    {
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 20);

        eventEntity.ReleaseSeats(20);

        eventEntity.AvailableSeats.Should().Be(100);
    }

    [Fact]
    public void ReleaseSeats_WhenResultWouldExceedTotalSeats_ThrowsDomainException()
    {
        // Nothing was ever reserved (AvailableSeats == TotalSeats already);
        // releasing seats on top of that would push AvailableSeats above TotalSeats.
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 100);

        var act = () => eventEntity.ReleaseSeats(1);

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed total seats limit*");
    }

    // ==========================================
    // Test helpers
    // ==========================================

    /// <summary>
    /// بيبني Event في حالة (status) معينة عن طريق تسلسل عمليات صحيح فعليًا
    /// (Draft -> Publish -> Cancel/Complete)، من غير أي reflection.
    /// </summary>
    private static Event BuildEventInStatus(EventStatus status)
    {
        switch (status)
        {
            case EventStatus.Draft:
                return EventTestFactory.CreateDraftEventWithSeats();

            case EventStatus.Published:
                return EventTestFactory.CreatePublishedEvent();

            case EventStatus.Cancelled:
                {
                    var e = EventTestFactory.CreatePublishedEvent();
                    e.Cancel();
                    return e;
                }

            case EventStatus.Completed:
                {
                    var start = Now.AddDays(-2);
                    var end = Now.AddDays(-1);
                    var e = EventTestFactory.CreatePublishedEvent(
                        startDateTime: start,
                        endDateTime: end,
                        publishTime: start.AddDays(-10));
                    e.Complete(Now);
                    return e;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(status));
        }
    }
}