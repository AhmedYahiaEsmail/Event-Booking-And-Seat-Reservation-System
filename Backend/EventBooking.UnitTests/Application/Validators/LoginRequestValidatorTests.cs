using EventBooking.Application.DTOs.Auth;
using EventBooking.Application.Validators.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace EventBooking.UnitTests.Application.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyEmail_HasValidationErrorForEmail()
    {
        var request = new LoginRequest("", "SomePassword1!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@no-local-part.com")]
    public void Validate_WithMalformedEmail_HasValidationErrorForEmail(string invalidEmail)
    {
        var request = new LoginRequest(invalidEmail, "SomePassword1!");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasValidationErrorForPassword()
    {
        var request = new LoginRequest("user@example.com", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithValidEmailAndPassword_HasNoValidationErrors()
    {
        var request = new LoginRequest("user@example.com", "AnyNonEmptyValue");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}