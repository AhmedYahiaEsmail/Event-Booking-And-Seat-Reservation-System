using EventBooking.API.Common;
using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // Browsing endpoints are public per product decision: catalog browsing (list,
    // upcoming, by-id) requires no authentication at all. [AllowAnonymous] is used
    // explicitly on each action below rather than simply omitting [Authorize], so the
    // intent stays unambiguous even if a controller-level [Authorize] is ever added
    // later.

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryParameters queryParams, CancellationToken cancellationToken)
    {
        var response = await _eventService.GetEventsAsync(queryParams, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventResponse>>.Success(response, "Events retrieved successfully."));
    }

    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] EventQueryParameters queryParams, CancellationToken cancellationToken)
    {
        var response = await _eventService.GetUpcomingEventsAsync(queryParams, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventResponse>>.Success(response, "Upcoming events retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _eventService.GetEventByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<EventResponse>.Success(response, "Event retrieved successfully."));
    }

    // Management endpoints (create/update/delete) are restricted to Admin, per the
    // "Admin dashboard to create new events and set seat limits" requirement in the
    // project documentation.

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var response = await _eventService.CreateEventAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = response.Id },
            ApiResponse<EventResponse>.Success(response, "Event created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var response = await _eventService.UpdateEventAsync(id, request, cancellationToken);
        return Ok(ApiResponse<EventResponse>.Success(response, "Event updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken)
    {
        // Soft delete: IEventService.DeleteEventAsync flips IsDeleted via the domain
        // entity; the row is never physically removed. A 204 has no body by definition,
        // so it is intentionally not wrapped in ApiResponse<T> (see implementation report).
        await _eventService.DeleteEventAsync(id, cancellationToken);
        return NoContent();
    }

    // Lifecycle endpoints — thin wrappers over IEventService.PublishEventAsync /
    // CancelEventAsync / CompleteEventAsync, which in turn call Event.Publish() /
    // .Cancel() / .Complete() on the domain entity. All invalid-transition rules (e.g.
    // publishing an already-published event, or completing one before its end time)
    // are enforced entirely inside those domain methods and surface as
    // DomainException -> 400 Bad Request via the existing GlobalExceptionMiddleware.
    // No additional validation or exception handling is added here, matching the
    // existing thinness of CreateEvent/UpdateEvent/DeleteEvent above.

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> PublishEvent(Guid id, CancellationToken cancellationToken)
    {
        var response = await _eventService.PublishEventAsync(id, cancellationToken);
        return Ok(ApiResponse<EventResponse>.Success(response, "Event published successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> CancelEvent(Guid id, CancellationToken cancellationToken)
    {
        var response = await _eventService.CancelEventAsync(id, cancellationToken);
        return Ok(ApiResponse<EventResponse>.Success(response, "Event cancelled successfully."));
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> CompleteEvent(Guid id, CancellationToken cancellationToken)
    {
        var response = await _eventService.CompleteEventAsync(id, cancellationToken);
        return Ok(ApiResponse<EventResponse>.Success(response, "Event marked as completed."));
    }
}