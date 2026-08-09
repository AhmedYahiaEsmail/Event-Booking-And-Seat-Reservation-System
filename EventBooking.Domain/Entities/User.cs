using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Entities;

public class User : AuditableEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    // Parameterless constructor for ORM (kept private to enforce encapsulation)
    private User() { }

    public User(Guid id, string firstName, string lastName, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name cannot be empty.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name cannot be empty.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Password hash cannot be empty.");

        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
