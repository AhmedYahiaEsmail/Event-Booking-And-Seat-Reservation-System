using EventBooking.Domain.Entities;

namespace EventBooking.UnitTests.TestHelpers;

public static class EventTestFactory
{
    public static readonly DateTimeOffset Now = new(2026, 8, 22, 12, 0, 0, TimeSpan.Zero);

    public static Event CreateEmptyDraftEvent(
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        Guid? id = null)
    {
        var start = startDateTime ?? Now.AddDays(7);
        var end = endDateTime ?? start.AddHours(2);

        return new Event
        {
            Id = id ?? Guid.NewGuid(),
            Title = "Test Event",
            Description = "Test Description",
            StartDateTime = start,
            EndDateTime = end,
            Location = "Test Location",
            SpeakerName = "Test Speaker",
            SpeakerBio = "Test Bio"
        };
    }

    public static Event CreateDraftEventWithSeats(
        int totalSeats = 100,
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        Guid? id = null)
    {
        var eventEntity = CreateEmptyDraftEvent(startDateTime, endDateTime, id);
        eventEntity.InitializeSeats(totalSeats);
        return eventEntity;
    }

    public static Event CreatePublishedEvent(
        int totalSeats = 100,
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        DateTimeOffset? publishTime = null,
        Guid? id = null)
    {
        var eventEntity = CreateDraftEventWithSeats(totalSeats, startDateTime, endDateTime, id);
        eventEntity.Publish(publishTime ?? Now);
        return eventEntity;
    }

    public static Event CreatePublishedEventWithReservedSeats(
        int totalSeats,
        int reservedSeats,
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        Guid? id = null)
    {
        var eventEntity = CreatePublishedEvent(totalSeats, startDateTime, endDateTime, id: id);
        if (reservedSeats > 0)
        {
            eventEntity.ReserveSeats(reservedSeats, Now);
        }

        return eventEntity;
    }
}