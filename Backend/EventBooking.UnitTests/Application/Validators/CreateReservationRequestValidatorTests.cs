using EventBooking.Application.DTOs.Reservations;
using EventBooking.Application.Validators.Reservations;
using FluentValidation.TestHelper;

namespace EventBooking.UnitTests.Application.Validators;

public class CreateReservationRequestValidatorTests
{
    private readonly CreateReservationRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidEventIdAndPositiveSeats_HasNoValidationErrors()
    {
        var request = new CreateReservationRequest(Guid.NewGuid(), 2);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEventId_HasValidationErrorForEventId()
    {
        var request = new CreateReservationRequest(Guid.Empty, 2);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EventId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveNumberOfSeats_HasValidationErrorForNumberOfSeats(int numberOfSeats)
    {
        var request = new CreateReservationRequest(Guid.NewGuid(), numberOfSeats);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NumberOfSeats);
    }
}