namespace EventBooking.Application.Interfaces.Common;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}