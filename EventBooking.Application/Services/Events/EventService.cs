using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Events;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Services.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
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

        eventEntity.Publish(DateTimeOffset.UtcNow);

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

    private static EventResponse MapToResponse(Domain.Entities.Event eventEntity)
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
            eventEntity.Status);
    }
}