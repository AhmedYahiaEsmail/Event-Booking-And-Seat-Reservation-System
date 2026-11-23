using System.ComponentModel.DataAnnotations;

namespace EventBooking.Infrastructure.Configurations;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT SecretKey is required.")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters long.")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "ExpiryInMinutes must be greater than zero.")]
    public int ExpiryInMinutes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RefreshTokenExpiryInDays must be greater than zero.")]
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}