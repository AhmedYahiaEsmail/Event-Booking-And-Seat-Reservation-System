using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Validators.Events;
using FluentValidation.TestHelper;

namespace EventBooking.UnitTests.Application.Validators;

public class CreateEventRequestValidatorTests
{
    private readonly CreateEventRequestValidator _validator = new();
    private static readonly DateTimeOffset Start = new(2026, 9, 1, 10, 0, 0, TimeSpan.Zero);

    private static CreateEventRequest ValidRequest() => new(
        Title: "Intro to Kubernetes",
        Description: "A hands-on workshop",
        StartDateTime: Start,
        EndDateTime: Start.AddHours(3),
        Location: "Cairo HQ",
        SpeakerName: "Mona Hassan",
        SpeakerBio: "Senior DevOps Engineer",
        TotalSeats: 50);

    [Fact]
    public void Validate_WithAllValidFields_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasValidationErrorForTitle()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleOver200Characters_HasValidationErrorForTitle()
    {
        var request = ValidRequest() with { Title = new string('x', 201) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyLocation_HasValidationErrorForLocation()
    {
        var request = ValidRequest() with { Location = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Validate_WithEmptySpeakerName_HasValidationErrorForSpeakerName()
    {
        var request = ValidRequest() with { SpeakerName = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SpeakerName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveTotalSeats_HasValidationErrorForTotalSeats(int totalSeats)
    {
        var request = ValidRequest() with { TotalSeats = totalSeats };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TotalSeats);
    }

    [Fact]
    public void Validate_WhenEndDateTimeIsBeforeStartDateTime_HasValidationErrorForEndDateTime()
    {
        var request = ValidRequest() with { EndDateTime = Start.AddHours(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndDateTime);
    }

    [Fact]
    public void Validate_WhenEndDateTimeEqualsStartDateTime_HasValidationErrorForEndDateTime()
    {
        // Boundary: EndDateTime must be strictly GreaterThan StartDateTime, equal is not allowed.
        var request = ValidRequest() with { EndDateTime = Start };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndDateTime);
    }
}