using EventBooking.Application.DTOs.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Interfaces.Events;

public interface IEventService
{
    Task<EventResponse> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<EventResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventResponse>> GetEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventResponse>> GetUpcomingEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<EventResponse> UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<EventResponse> PublishEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventResponse> CancelEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventResponse> CompleteEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}