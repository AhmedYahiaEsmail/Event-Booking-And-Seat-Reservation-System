using EventBooking.Application.DTOs.Reservations;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Application.Interfaces.Reservations;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventBooking.Application.Services.Reservations;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponse> ReserveSeatsAsync(
        Guid currentUserId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Event.ReserveSeats does not itself guard against a non-positive seat count
        // (pre-existing domain gap — see implementation report, Out-of-Scope Findings).
        // Enforced here directly, rather than only relying on a future controller's
        // validation pipeline, so the rule holds no matter how this service is invoked.
        if (request.NumberOfSeats <= 0)
        {
            throw new DomainException("Number of seats must be greater than zero.");
        }

        var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(request.EventId);
        }

        // Event.ReserveSeats is the single source of truth for booking invariants:
        // must be Published, start time must not have passed, and there must be enough
        // AvailableSeats. Ordinary business-rule violations surface as DomainException
        // (-> 400 via the existing middleware), independent of the concurrency path below.
        eventEntity.ReserveSeats(request.NumberOfSeats, DateTimeOffset.UtcNow);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            EventId = eventEntity.Id,
            NumberOfSeats = request.NumberOfSeats,
        };

        await _reservationRepository.AddAsync(reservation, cancellationToken);

        // Single SaveChangesAsync call: the Event's seat decrement and the new
        // Reservation insert commit together in one implicit database transaction.
        // Event.RowVersion (loaded as part of the tracked eventEntity above) is the
        // concurrency guard — if another request already changed this exact Event row
        // since it was loaded here, EF Core throws DbUpdateConcurrencyException and
        // NEITHER change commits. See the implementation report for the full
        // concurrent-booking correctness argument (Section 4).
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(reservation, eventEntity);
    }

    public async Task CancelReservationAsync(
        Guid currentUserId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            throw new ReservationNotFoundException(reservationId);
        }

        if (reservation.UserId != currentUserId)
        {
            throw new ReservationAccessDeniedException();
        }

        // Domain method is the single source of truth: throws DomainException if the
        // reservation is already Cancelled, guarding sequential double-cancel attempts
        // (a second request arriving after the first has already fully committed).
        reservation.Cancel();

        if (reservation.Event is null)
        {
            // Should not normally happen: ReservationRepository.GetByIdAsync has a
            // narrowly scoped fallback (see implementation report, Section 8) that loads
            // the Event even if it has been soft-deleted. A null Event here means the
            // EventId genuinely does not resolve to any row.
            throw new EventNotFoundException(reservation.EventId);
        }

        reservation.Event.ReleaseSeats(reservation.NumberOfSeats);

        // Single SaveChangesAsync call: the Reservation's status change and the Event's
        // seat release commit together in one transaction. Event.RowVersion (loaded as
        // part of the tracked Event included on `reservation`) also protects against a
        // second, concurrent cancellation of this same reservation racing on the same
        // Event row — see implementation report Sections 4 and 17.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetUserReservationsAsync(
        Guid currentUserId,
        ReservationStatus? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetUserReservationsAsync(
            currentUserId, status, fromDate, toDate, pageNumber, pageSize, cancellationToken);

        return reservations.Select(r => MapToResponse(r, r.Event)).ToList().AsReadOnly();
    }

    private static ReservationResponse MapToResponse(Reservation reservation, Event? eventEntity)
    {
        return new ReservationResponse(
            reservation.Id,
            reservation.EventId,
            eventEntity?.Title ?? string.Empty,
            eventEntity?.StartDateTime ?? default,
            eventEntity?.Location ?? string.Empty,
            reservation.UserId,
            reservation.NumberOfSeats,
            reservation.BookingDateTime,
            reservation.Status);
    }
}