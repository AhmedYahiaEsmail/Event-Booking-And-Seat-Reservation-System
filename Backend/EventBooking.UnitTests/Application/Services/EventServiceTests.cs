using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Common;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Application.Services.Events;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.UnitTests.TestHelpers;
using FluentAssertions;
using Moq;

namespace EventBooking.UnitTests.Application.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(EventTestFactory.Now);
        _sut = new EventService(_eventRepositoryMock.Object, _unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    // ==========================================
    // CreateEventAsync
    // ==========================================

    [Fact]
    public async Task CreateEventAsync_WithValidRequest_InitializesSeatsAndPersistsAndReturnsMappedResponse()
    {
        var start = EventTestFactory.Now.AddDays(10);
        var end = start.AddHours(3);
        var request = new CreateEventRequest(
            Title: "  .NET Deep Dive  ",
            Description: "desc",
            StartDateTime: start,
            EndDateTime: end,
            Location: "Cairo",
            SpeakerName: "  Ahmed  ",
            SpeakerBio: "bio",
            TotalSeats: 150);

        Event? capturedEvent = null;
        _eventRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .Callback<Event, CancellationToken>((e, _) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateEventAsync(request);

        // Trimming behavior
        response.Title.Should().Be(".NET Deep Dive");
        response.SpeakerName.Should().Be("Ahmed");
        response.TotalSeats.Should().Be(150);
        response.AvailableSeats.Should().Be(150);
        response.Status.Should().Be(EventStatus.Draft);

        capturedEvent.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // GetEventByIdAsync
    // ==========================================

    [Fact]
    public async Task GetEventByIdAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);

        var act = async () => await _sut.GetEventByIdAsync(id);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventExists_ReturnsCorrectlyMappedResponse()
    {
        var id = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 80, reservedSeats: 20, id: id);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);

        var response = await _sut.GetEventByIdAsync(id);

        response.Id.Should().Be(id);
        response.TotalSeats.Should().Be(80);
        response.AvailableSeats.Should().Be(60);
        response.Status.Should().Be(EventStatus.Published);
    }

    // ==========================================
    // GetEventsAsync / GetUpcomingEventsAsync — mapping over a list
    // ==========================================

    [Fact]
    public async Task GetEventsAsync_MapsEveryEventInResultSet_PreservingOrder()
    {
        var e1 = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 10);
        var e2 = EventTestFactory.CreatePublishedEvent(totalSeats: 20);
        var queryParams = new EventQueryParameters();
        _eventRepositoryMock
            .Setup(r => r.GetEventsAsync(queryParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event> { e1, e2 });

        var responses = await _sut.GetEventsAsync(queryParams);

        responses.Should().HaveCount(2);
        responses[0].Id.Should().Be(e1.Id);
        responses[1].Id.Should().Be(e2.Id);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmptyList_WhenRepositoryReturnsNoEvents()
    {
        var queryParams = new EventQueryParameters();
        _eventRepositoryMock
            .Setup(r => r.GetUpcomingEventsAsync(queryParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());

        var responses = await _sut.GetUpcomingEventsAsync(queryParams);

        responses.Should().BeEmpty();
    }

    // ==========================================
    // UpdateEventAsync — including the optimistic-concurrency wiring
    // ==========================================

    [Fact]
    public async Task UpdateEventAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);
        var request = BuildUpdateRequest();

        var act = async () => await _sut.UpdateEventAsync(id, request);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task UpdateEventAsync_WhenTotalSeatsChanges_CallsUpdateTotalSeatsAndReflectsNewAvailableSeats()
    {
        var id = Guid.NewGuid();
        // 100 total, 30 reserved -> 70 available
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 100, reservedSeats: 30, id: id);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        var request = BuildUpdateRequest(totalSeats: 150);

        var response = await _sut.UpdateEventAsync(id, request);

        // reservedSeats (30) stays fixed, new available = 150 - 30 = 120
        response.TotalSeats.Should().Be(150);
        response.AvailableSeats.Should().Be(120);
    }

    [Fact]
    public async Task UpdateEventAsync_AlwaysPassesTheClientSuppliedRowVersion_AsTheOptimisticConcurrencyCheck()
    {
        // This is the crux of optimistic concurrency here: the RowVersion the client
        // read (and sends back on update) must be what's used as the "original" value
        // EF Core compares against, not whatever happens to be currently loaded.
        var id = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 50, id: id);
        var clientRowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        var request = BuildUpdateRequest(totalSeats: 50, rowVersion: clientRowVersion);

        await _sut.UpdateEventAsync(id, request);

        _eventRepositoryMock.Verify(
            r => r.SetOriginalRowVersion(eventEntity, clientRowVersion),
            Times.Once);
    }

    // ==========================================
    // Lifecycle: Publish / Cancel / Complete / Delete
    // ==========================================

    [Fact]
    public async Task PublishEventAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);

        var act = async () => await _sut.PublishEventAsync(id);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task PublishEventAsync_WhenEventExistsAndIsDraft_PublishesAndPersists()
    {
        var id = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 10, id: id);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);

        var response = await _sut.PublishEventAsync(id);

        response.Status.Should().Be(EventStatus.Published);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_SoftDeletesAndPersists_ButDoesNotCallRemove()
    {
        var id = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 10, id: id);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);

        await _sut.DeleteEventAsync(id);

        eventEntity.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static UpdateEventRequest BuildUpdateRequest(int totalSeats = 100, byte[]? rowVersion = null)
    {
        var start = EventTestFactory.Now.AddDays(5);
        return new UpdateEventRequest(
            Title: "Updated Title",
            Description: "Updated Description",
            StartDateTime: start,
            EndDateTime: start.AddHours(2),
            Location: "Updated Location",
            SpeakerName: "Updated Speaker",
            SpeakerBio: "Updated Bio",
            TotalSeats: totalSeats,
            RowVersion: rowVersion ?? new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 });
    }
}