using EventBooking.Application.DTOs.Events;
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

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Tracking is preserved intentionally: EventService.UpdateEventAsync mutates
        // the returned entity and relies on change tracking before UnitOfWork.SaveChangesAsync().
        // The global soft-delete query filter (IsDeleted) defined in EventConfiguration
        // is applied automatically and is not bypassed here.
        return await _context.Events
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsAsync(
        EventQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        // Read-only listing query: AsNoTracking avoids the overhead of change tracking.
        // The global soft-delete query filter is applied automatically and is not bypassed.
        IQueryable<Event> query = _context.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.Trim();

            query = query.Where(e =>
                e.Title.Contains(searchTerm) ||
                (e.Description != null && e.Description.Contains(searchTerm)) ||
                e.Location.Contains(searchTerm) ||
                e.SpeakerName.Contains(searchTerm));
        }

        if (queryParams.Status.HasValue)
        {
            query = query.Where(e => e.Status == queryParams.Status.Value);
        }

        if (queryParams.FromDate.HasValue)
        {
            query = query.Where(e => e.StartDateTime >= queryParams.FromDate.Value);
        }

        if (queryParams.ToDate.HasValue)
        {
            query = query.Where(e => e.StartDateTime <= queryParams.ToDate.Value);
        }

        // Deterministic ordering is required before Skip/Take to get stable pagination.
        query = query
            .OrderBy(e => e.StartDateTime)
            .ThenBy(e => e.Id);

        // Defensive clamping only (not business validation): guards Skip/Take against
        // non-positive values that would otherwise throw or return unexpected results.
        var pageNumber = queryParams.PageNumber < 1 ? 1 : queryParams.PageNumber;
        var pageSize = queryParams.PageSize < 1 ? 10 : queryParams.PageSize;

        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(
        EventQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        // "Upcoming" is a fixed business definition (Published + future StartDateTime),
        // not something the caller can override, so only pagination is taken from
        // queryParams here — SearchTerm/Status/FromDate/ToDate are intentionally unused
        // for this method rather than layering ad-hoc extra filtering onto a named query.
        var now = DateTimeOffset.UtcNow;

        IQueryable<Event> query = _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published && e.StartDateTime > now)
            .OrderBy(e => e.StartDateTime)
            .ThenBy(e => e.Id);

        var pageNumber = queryParams.PageNumber < 1 ? 1 : queryParams.PageNumber;
        var pageSize = queryParams.PageSize < 1 ? 10 : queryParams.PageSize;

        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        // SaveChanges is intentionally not called here; persistence is committed
        // by IUnitOfWork.SaveChangesAsync() from the calling service.
        await _context.Events.AddAsync(eventEntity, cancellationToken);
    }

    public void SetOriginalRowVersion(Event eventEntity, byte[] rowVersion)
    {
        // Forces EF Core to use the client-supplied RowVersion (the value the client
        // read before editing) as the "original" value for the concurrency check,
        // overriding whatever value was loaded when the entity was fetched in this
        // request. This is what makes the WHERE clause on UPDATE compare against the
        // client's version rather than the just-loaded (possibly newer) one. No
        // database I/O happens here; SaveChangesAsync (via IUnitOfWork) is what
        // actually performs and enforces the check.
        _context.Entry(eventEntity).Property(e => e.RowVersion).OriginalValue = rowVersion;
    }
}