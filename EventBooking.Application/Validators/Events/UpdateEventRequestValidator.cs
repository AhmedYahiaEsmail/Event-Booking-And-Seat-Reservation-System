using EventBooking.Application.DTOs.Events;
using FluentValidation;

namespace EventBooking.Application.Validators.Events;

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(300).WithMessage("Location must not exceed 300 characters.");

        RuleFor(x => x.SpeakerName)
            .NotEmpty().WithMessage("Speaker name is required.")
            .MaximumLength(150).WithMessage("Speaker name must not exceed 150 characters.");

        RuleFor(x => x.SpeakerBio)
            .MaximumLength(1000).WithMessage("Speaker bio must not exceed 1000 characters.");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Total seats must be greater than zero.");

        RuleFor(x => x.EndDateTime)
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("End date time must be after start date time.");
    }
}