using EventBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
