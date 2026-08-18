using EventBooking.Application.DTOs.Events;
using EventBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Interfaces.Persistence;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);

    // Persistence-only concurrency plumbing: marks the value the caller last read as the
    // "original" concurrency token EF Core should compare against on SaveChangesAsync.
    // No I/O happens here and SaveChangesAsync is never called from this method.
    void SetOriginalRowVersion(Event eventEntity, byte[] rowVersion);
}