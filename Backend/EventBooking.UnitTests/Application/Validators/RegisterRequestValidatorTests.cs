using EventBooking.Application.DTOs.Auth;
using EventBooking.Application.Validators.Auth;
using FluentValidation.TestHelper;

namespace EventBooking.UnitTests.Application.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest(string password = "P@ssw0rd1") =>
        new("Sara", "Ahmed", "sara@example.com", password);

    [Fact]
    public void Validate_WithAllValidFields_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFirstName_HasValidationErrorForFirstName()
    {
        var request = ValidRequest() with { FirstName = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithFirstNameOver50Characters_HasValidationErrorForFirstName()
    {
        var request = ValidRequest() with { FirstName = new string('a', 51) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasValidationErrorForEmail()
    {
        var request = ValidRequest() with { Email = "not-an-email" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // Password complexity — every rule tested independently, each with a password that
    // violates ONLY that one rule (everything else about it is compliant), so a failure
    // pinpoints exactly which .Matches()/.MinimumLength() rule in
    // RegisterRequestValidator broke, and the exact message can be asserted precisely.
    [Theory]
    [InlineData("Ab1!xyz", "Password must be at least 8 characters long.")]          // 7 chars, otherwise valid
    [InlineData("ab1!wxyz", "Password must contain at least one uppercase letter.")] // 8 chars, no uppercase
    [InlineData("AB1!WXYZ", "Password must contain at least one lowercase letter.")] // 8 chars, no lowercase
    [InlineData("Abc!WXYZ", "Password must contain at least one digit.")]            // 8 chars, no digit
    [InlineData("Abc12345", "Password must contain at least one special character.")] // 8 chars, no special char
    public void Validate_WithPasswordViolatingExactlyOneComplexityRule_HasThatSpecificValidationError(string invalidPassword, string expectedMessage)
    {
        var request = ValidRequest(invalidPassword);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Validate_WithStrongPassword_HasNoValidationErrorForPassword()
    {
        var request = ValidRequest("Str0ng!Passw0rd");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}