using EventBooking.Application.DTOs.Reservations;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Application.Services.Reservations;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using EventBooking.UnitTests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventBooking.UnitTests.Application.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _sut = new ReservationService(
            _reservationRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    // ==========================================
    // ReserveSeatsAsync
    // ==========================================

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task ReserveSeatsAsync_WhenNumberOfSeatsIsNotPositive_ThrowsDomainException_WithoutTouchingTheEvent(int numberOfSeats)
    {
        // Service-level guard: Event.ReserveSeats itself doesn't validate a non-positive
        // count, so this must be enforced here regardless of caller. Because it's
        // enforced before the event is even loaded, GetByIdAsync should never be called.
        var request = new CreateReservationRequest(Guid.NewGuid(), numberOfSeats);

        var act = async () => await _sut.ReserveSeatsAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*greater than zero*");
        _eventRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReserveSeatsAsync_WhenEventDoesNotExist_ThrowsEventNotFoundException()
    {
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);
        var request = new CreateReservationRequest(eventId, 2);

        var act = async () => await _sut.ReserveSeatsAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task ReserveSeatsAsync_WhenNotEnoughAvailableSeats_PropagatesDomainException_AndDoesNotPersistAnything()
    {
        // Only 3 seats left, someone asks for 5.
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 10, reservedSeats: 7);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        var request = new CreateReservationRequest(eventEntity.Id, 5);

        var act = async () => await _sut.ReserveSeatsAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Not enough available seats*");
        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        eventEntity.AvailableSeats.Should().Be(3, "a rejected reservation must not partially decrement seats");
    }

    [Fact]
    public async Task ReserveSeatsAsync_WhenEventIsNotPublished_PropagatesDomainException()
    {
        var eventEntity = EventTestFactory.CreateDraftEventWithSeats(totalSeats: 10);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        var request = new CreateReservationRequest(eventEntity.Id, 1);

        var act = async () => await _sut.ReserveSeatsAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Cannot reserve seats unless the event is Published*");
    }

    [Fact]
    public async Task ReserveSeatsAsync_OnSuccess_DecrementsSeats_LinksReservationToCurrentUser_AndPersistsOnce()
    {
        var currentUserId = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 50);
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        var request = new CreateReservationRequest(eventEntity.Id, 4);

        Reservation? capturedReservation = null;
        _reservationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((res, _) => capturedReservation = res)
            .Returns(Task.CompletedTask);

        var response = await _sut.ReserveSeatsAsync(currentUserId, request);

        eventEntity.AvailableSeats.Should().Be(46);
        capturedReservation.Should().NotBeNull();
        capturedReservation!.UserId.Should().Be(currentUserId);
        capturedReservation.EventId.Should().Be(eventEntity.Id);
        capturedReservation.NumberOfSeats.Should().Be(4);
        capturedReservation.Status.Should().Be(ReservationStatus.Confirmed);

        response.UserId.Should().Be(currentUserId);
        response.EventTitle.Should().Be(eventEntity.Title);
        response.NumberOfSeats.Should().Be(4);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // CancelReservationAsync
    // ==========================================

    [Fact]
    public async Task CancelReservationAsync_WhenReservationDoesNotExist_ThrowsReservationNotFoundException()
    {
        var reservationId = Guid.NewGuid();
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);

        var act = async () => await _sut.CancelReservationAsync(Guid.NewGuid(), reservationId);

        await act.Should().ThrowAsync<ReservationNotFoundException>();
    }

    [Fact]
    public async Task CancelReservationAsync_WhenReservationBelongsToAnotherUser_ThrowsReservationAccessDeniedException()
    {
        var ownerId = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 20, reservedSeats: 5);
        var reservation = ReservationTestFactory.CreateConfirmedReservation(userId: ownerId, attachedEvent: eventEntity, numberOfSeats: 5);
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        var someoneElseId = Guid.NewGuid();
        var act = async () => await _sut.CancelReservationAsync(someoneElseId, reservation.Id);

        await act.Should().ThrowAsync<ReservationAccessDeniedException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 20, reservedSeats: 5);
        var reservation = ReservationTestFactory.CreateConfirmedReservation(userId: userId, attachedEvent: eventEntity, numberOfSeats: 5);
        reservation.Cancel(); // already cancelled beforehand
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        var act = async () => await _sut.CancelReservationAsync(userId, reservation.Id);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public async Task CancelReservationAsync_WhenEventNavigationIsNull_ThrowsEventNotFoundException_AndDoesNotPersist()
    {
        // Mirrors the narrow edge case documented in ReservationService: reservation.Event
        // came back null (e.g. the Event row genuinely can't be resolved).
        var userId = Guid.NewGuid();
        var reservation = ReservationTestFactory.CreateConfirmedReservation(userId: userId, numberOfSeats: 3);
        // attachedEvent intentionally omitted -> reservation.Event stays null
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        var act = async () => await _sut.CancelReservationAsync(userId, reservation.Id);

        await act.Should().ThrowAsync<EventNotFoundException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_OnSuccess_ReleasesSeatsBackToEvent_AndPersistsOnce()
    {
        var userId = Guid.NewGuid();
        // 20 total, 8 reserved (this reservation is 5 of those 8) -> after cancel: 20-3=17 reserved -> available 15
        var eventEntity = EventTestFactory.CreatePublishedEventWithReservedSeats(totalSeats: 20, reservedSeats: 8);
        var reservation = ReservationTestFactory.CreateConfirmedReservation(userId: userId, attachedEvent: eventEntity, numberOfSeats: 5);
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        await _sut.CancelReservationAsync(userId, reservation.Id);

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        eventEntity.AvailableSeats.Should().Be(17); // 12 + 5 released
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // GetUserReservationsAsync
    // ==========================================

    [Fact]
    public async Task GetUserReservationsAsync_PassesAllFiltersThroughToRepository_AndMapsResults()
    {
        var userId = Guid.NewGuid();
        var eventEntity = EventTestFactory.CreatePublishedEvent(totalSeats: 30);
        var reservation = ReservationTestFactory.CreateConfirmedReservation(userId: userId, attachedEvent: eventEntity, numberOfSeats: 2);
        var fromDate = EventTestFactory.Now.AddDays(-30);
        var toDate = EventTestFactory.Now;

        _reservationRepositoryMock
            .Setup(r => r.GetUserReservationsAsync(
                userId, ReservationStatus.Confirmed, fromDate, toDate, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservation> { reservation });

        var responses = await _sut.GetUserReservationsAsync(
            userId, ReservationStatus.Confirmed, fromDate, toDate, pageNumber: 2, pageSize: 5);

        responses.Should().ContainSingle();
        responses[0].EventTitle.Should().Be(eventEntity.Title);
        responses[0].UserId.Should().Be(userId);
    }
}