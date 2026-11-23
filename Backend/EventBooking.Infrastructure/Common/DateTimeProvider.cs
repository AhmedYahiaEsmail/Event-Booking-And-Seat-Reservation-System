using EventBooking.Application.Interfaces.Common;

namespace EventBooking.Infrastructure.Common;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}