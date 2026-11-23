using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using EventBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        // included in the same tracked graph. The upcoming (TASK-05) cancellation
        // workflow mutates both the Reservation (Cancel()) and its Event (ReleaseSeats())
        // within this single DbContext instance so both changes commit together through
        // one IUnitOfWork.SaveChangesAsync() call / one database transaction.
        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (reservation is not null && reservation.Event is null)
        {
            // TASK-05 soft-delete decision: the Include above respects Event's global
            // soft-delete query filter, so a soft-deleted Event comes back as null even
            // though reservation.EventId still points at a real row. Cancellation must
            // still be able to release seats on that Event regardless of its soft-delete
            // state (see implementation report Section 8 for the full reasoning).
            //
            // This is a narrowly scoped, deliberate fallback: IgnoreQueryFilters() is
            // applied only to this single follow-up query for the one specific Event row
            // already referenced by this Reservation's EventId. It never affects the
            // Reservation query above, and the global query filter itself is untouched.
            // Once loaded, EF Core's relationship fixup automatically wires this Event
            // into reservation.Event because both are tracked in the same DbContext.
            await _context.Events
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(e => e.Id == reservation.EventId, cancellationToken);
        }

        return reservation;
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