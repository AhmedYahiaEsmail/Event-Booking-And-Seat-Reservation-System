using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Token);