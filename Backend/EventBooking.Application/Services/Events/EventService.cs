using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Common;
using EventBooking.Application.Interfaces.Events;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;

namespace EventBooking.Application.Services.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EventService(IEventRepository eventRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<EventResponse> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Using Object Initialization (Relies on FluentValidation for safety)
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            Location = request.Location.Trim(),
            SpeakerName = request.SpeakerName.Trim(),
            SpeakerBio = request.SpeakerBio?.Trim(),
        };

        // 2. Execute Domain Logic to initialize available seats
        eventEntity.InitializeSeats(request.TotalSeats);

        await _eventRepository.AddAsync(eventEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // eventEntity.RowVersion is populated by EF Core / SQL Server as part of the
        // insert above, so the mapped response below already carries the current value.

        return MapToResponse(eventEntity);
    }

    public async Task<EventResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        return MapToResponse(eventEntity);
    }

    public async Task<IReadOnlyList<EventResponse>> GetEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetEventsAsync(queryParams, cancellationToken);
        return events.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<EventResponse>> GetUpcomingEventsAsync(EventQueryParameters queryParams, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetUpcomingEventsAsync(queryParams, cancellationToken);
        return events.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<EventResponse> UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        // 1. Update basic details
        eventEntity.UpdateDetails(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.StartDateTime,
            request.EndDateTime,
            request.Location.Trim(),
            request.SpeakerName.Trim(),
            request.SpeakerBio?.Trim());

        // 2. Update total seats separately (As per domain design)
        if (eventEntity.TotalSeats != request.TotalSeats)
        {
            eventEntity.UpdateTotalSeats(request.TotalSeats);
        }

        // 3. Optimistic concurrency: use the RowVersion the client read as the expected
        // ("original") value for this update. If another request has already changed
        // the row, SaveChangesAsync below throws DbUpdateConcurrencyException — the
        // database is the authority here, this is not an application-level array
        // comparison. The exception is intentionally left unhandled; API-level
        // translation is a later task.
        _eventRepository.SetOriginalRowVersion(eventEntity, request.RowVersion);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventEntity);
    }

    public async Task<EventResponse> PublishEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        eventEntity.Publish(_dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventEntity);
    }

    public async Task<EventResponse> CancelEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        eventEntity.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventEntity);
    }

    public async Task<EventResponse> CompleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        eventEntity.Complete(_dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventEntity);
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity is null)
        {
            throw new EventNotFoundException(id);
        }

        // Soft delete only: flips IsDeleted/DeletedAt via AuditableEntity. The row is
        // never physically removed (no DbSet.Remove call), and the existing global
        // query filter (!IsDeleted) on Event will exclude it from future reads.
        eventEntity.MarkAsDeleted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static EventResponse MapToResponse(Event eventEntity)
    {
        return new EventResponse(
            eventEntity.Id,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.StartDateTime,
            eventEntity.EndDateTime,
            eventEntity.Location,
            eventEntity.SpeakerName,
            eventEntity.SpeakerBio,
            eventEntity.TotalSeats,
            eventEntity.AvailableSeats,
            eventEntity.Status,
            eventEntity.RowVersion);
    }
}