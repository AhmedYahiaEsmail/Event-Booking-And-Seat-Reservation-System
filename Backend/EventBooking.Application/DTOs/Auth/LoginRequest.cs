using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password);
