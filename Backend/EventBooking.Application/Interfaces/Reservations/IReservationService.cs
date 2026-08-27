using EventBooking.Application.DTOs.Reservations;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.Interfaces.Reservations;

public interface IReservationService
{
    // currentUserId is supplied by the caller (the future ReservationsController, reading
    // the ClaimTypes.NameIdentifier claim already issued by JwtTokenGenerator) rather than
    // being resolved inside this service. Application has no dependency on HttpContext /
    // ClaimsPrincipal today, and this keeps it that way — see implementation report for
    // why no ICurrentUserService abstraction was introduced in this task.

    Task<ReservationResponse> ReserveSeatsAsync(
        Guid currentUserId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);

    Task CancelReservationAsync(
        Guid currentUserId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReservationResponse>> GetUserReservationsAsync(
        Guid currentUserId,
        ReservationStatus? status = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}