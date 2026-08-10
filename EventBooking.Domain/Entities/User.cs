using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class User : AuditableEntity
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required UserRole Role { get; init; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();
}
