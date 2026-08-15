using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventBooking.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Tracking is preserved deliberately (no AsNoTracking), and the related Event is
        // included in the same tracked graph. The upcoming cancellation workflow needs to
        // mutate both the Reservation (Cancel()) and its Event (ReleaseSeats()) within a
        // single DbContext instance so both changes commit together through one
        // IUnitOfWork.SaveChangesAsync() call / one database transaction. The global
        // soft-delete query filters on both Reservation and Event are not bypassed here.
        return await _context.Reservations
            .Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetUserReservationsAsync(
        Guid userId,
        ReservationStatus? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Read-only history/dashboard query. UserId is always applied as a mandatory
        // equality filter (never optional), so this method structurally cannot be used
        // to read another user's reservations regardless of which other filters are
        // supplied by the caller.
        IQueryable<Reservation> query = _context.Reservations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.BookingDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.BookingDateTime <= toDate.Value);
        }

        // Deterministic ordering before Skip/Take: most recent booking first, Id as a
        // stable tiebreaker.
        query = query
            .OrderByDescending(r => r.BookingDateTime)
            .ThenBy(r => r.Id);

        // Defensive clamping only (not business validation), consistent with
        // EventRepository's existing pagination-guard pattern.
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 10 : pageSize;

        query = query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        // SaveChanges is intentionally not called here; commit responsibility remains
        // with IUnitOfWork.SaveChangesAsync(), consistent with EventRepository.AddAsync.
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }
}