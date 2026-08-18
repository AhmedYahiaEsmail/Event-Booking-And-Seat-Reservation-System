using EventBooking.API.Common;
using EventBooking.Application.DTOs.Reservations;
using EventBooking.Application.Interfaces.Reservations;
using EventBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventBooking.API.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ReserveSeats([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var response = await _reservationService.ReserveSeatsAsync(currentUserId, request, cancellationToken);

        // No "get reservation by id" endpoint exists in this API contract to anchor a
        // CreatedAtAction/Location header to, so a plain 201 is returned with the created
        // resource in the body via the existing ApiResponse<T> envelope, per the task's
        // instruction to fall back to the existing response convention in that case.
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ReservationResponse>.Success(response, "Reservation created successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        // Ownership/state validation (does this reservation belong to currentUserId, can
        // it be cancelled) lives entirely inside ReservationService/domain — the
        // controller only supplies the route id and the authenticated user's id.
        await _reservationService.CancelReservationAsync(currentUserId, id, cancellationToken);

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] ReservationStatus? status,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var response = await _reservationService.GetUserReservationsAsync(
            currentUserId, status, fromDate, toDate, pageNumber, pageSize, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<ReservationResponse>>.Success(response, "Reservations retrieved successfully."));
    }

    // JwtTokenGenerator (EventBooking.Infrastructure/Identity) always issues
    // ClaimTypes.NameIdentifier as the authenticated user's Guid Id. [Authorize] on each
    // action above guarantees a validated token was already accepted before the action
    // body runs, so this claim is trusted here rather than re-validated defensively.
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!);
    }
}