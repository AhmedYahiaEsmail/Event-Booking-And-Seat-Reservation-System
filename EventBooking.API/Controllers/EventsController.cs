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

    // Browsing endpoints require an authenticated account but are not restricted to a
    // single role: RequireAdminRole/RequireUserRole check for an exact role match, so
    // using either alone would lock out the other role from browsing the catalog. Since
    // the project documentation doesn't scope catalog browsing to one specific role,
    // plain [Authorize] (any authenticated user) is used here rather than inventing a
    // new policy.

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryParameters queryParams, CancellationToken cancellationToken)
    {
        var response = await _eventService.GetEventsAsync(queryParams, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventResponse>>.Success(response, "Events retrieved successfully."));
    }

    [HttpGet("upcoming")]
    [Authorize]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] EventQueryParameters queryParams, CancellationToken cancellationToken)
    {
        var response = await _eventService.GetUpcomingEventsAsync(queryParams, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventResponse>>.Success(response, "Upcoming events retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
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
}