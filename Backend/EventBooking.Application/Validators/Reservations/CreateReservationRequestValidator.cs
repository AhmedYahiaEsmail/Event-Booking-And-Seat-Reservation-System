using EventBooking.Application.DTOs.Reservations;
using FluentValidation;

namespace EventBooking.Application.Validators.Reservations;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("EventId is required.");

        RuleFor(x => x.NumberOfSeats)
            .GreaterThan(0).WithMessage("NumberOfSeats must be greater than zero.");
    }
}