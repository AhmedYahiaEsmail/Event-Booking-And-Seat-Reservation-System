using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventBooking.Application.Interfaces.Persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // No existing pagination/query DTO fits Reservation filtering (EventQueryParameters
    // is Event-specific: its Status is typed to EventStatus, and SearchTerm has no
    // Reservation equivalent). Rather than invent a new DTO — which TASK-04 explicitly
    // excludes ("DO NOT implement: Reservation DTOs") — this uses plain parameters
    // mirroring the same filter/pagination shape (status, date range, page, size).
    Task<IReadOnlyList<Reservation>> GetUserReservationsAsync(
        Guid userId,
        ReservationStatus? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}