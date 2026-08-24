using EventBooking.Domain.Entities;

namespace EventBooking.UnitTests.TestHelpers;

/// <summary>
/// كل الـ methods هنا بتبني الـ Event عن طريق الـ public API بتاع الـ domain بس
/// (InitializeSeats / Publish / ReserveSeats) - من غير أي reflection أو تلاعب مباشر
/// في الـ private setters. ده يضمن إن أي تيست بيستخدم الـ factory ده فعليًا بيمر
/// بنفس المسار اللي المستخدم الحقيقي هيمر بيه.
/// </summary>
public static class EventTestFactory
{
    public static readonly DateTimeOffset Now = new(2026, 8, 22, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Event جديد لسه معملوش InitializeSeats عليه (TotalSeats = 0, AvailableSeats = 0, Status = Draft).
    /// مفيد لتيست InitializeSeats نفسها وحالة "Cannot publish before initializing seats".
    /// </summary>
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

    /// <summary>
    /// Draft event وseats متعملها initialize (لسه Draft، لسه مش Published).
    /// </summary>
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

    /// <summary>
    /// Event متعمله Publish فعليًا (الحالة الافتراضية اللي هيتحجز منها في الغالب).
    /// </summary>
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

    /// <summary>
    /// Event Published وليه عدد معين من الـ seats المحجوزة بالفعل (AvailableSeats أقل من TotalSeats).
    /// </summary>
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