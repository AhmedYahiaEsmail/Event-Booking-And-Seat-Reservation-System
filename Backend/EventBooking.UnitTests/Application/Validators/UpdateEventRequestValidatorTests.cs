using EventBooking.Application.DTOs.Events;
using EventBooking.Application.Validators.Events;
using FluentValidation.TestHelper;
using Xunit;

namespace EventBooking.UnitTests.Application.Validators;

public class UpdateEventRequestValidatorTests
{
    private readonly UpdateEventRequestValidator _validator = new();
    private static readonly DateTimeOffset Start = new(2026, 9, 1, 10, 0, 0, TimeSpan.Zero);

    private static UpdateEventRequest ValidRequest() => new(
        Title: "Intro to Kubernetes",
        Description: "A hands-on workshop",
        StartDateTime: Start,
        EndDateTime: Start.AddHours(3),
        Location: "Cairo HQ",
        SpeakerName: "Mona Hassan",
        SpeakerBio: "Senior DevOps Engineer",
        TotalSeats: 50,
        RowVersion: new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

    [Fact]
    public void Validate_WithAllValidFields_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRowVersion_HasValidationErrorForRowVersion()
    {
        // Critical for optimistic concurrency: an empty RowVersion means the client never
        // actually read the event before trying to update it, so the request must be rejected
        // here rather than reaching EF Core's concurrency check with a meaningless token.
        var request = ValidRequest() with { RowVersion = Array.Empty<byte>() };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasValidationErrorForTitle()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveTotalSeats_HasValidationErrorForTotalSeats(int totalSeats)
    {
        var request = ValidRequest() with { TotalSeats = totalSeats };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TotalSeats);
    }

    [Fact]
    public void Validate_WhenEndDateTimeIsNotAfterStartDateTime_HasValidationErrorForEndDateTime()
    {
        var request = ValidRequest() with { EndDateTime = Start };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndDateTime);
    }
}